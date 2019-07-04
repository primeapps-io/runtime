using System;//test
using System.IO;
using System.Reflection;
using System.Text;
using Devart.Data.PostgreSql;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.Model.Helpers;
using PrimeApps.Studio.ActionFilters;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Jobs
{
    public class Dump
    {
        private static IConfiguration _configuration;
        private IGiteaHelper _giteaHelper;

        public Dump(IConfiguration configuration, IGiteaHelper giteaHelper)
        {
            _configuration = configuration;
            _giteaHelper = giteaHelper;
        }

        [QueueCustom]
        public void Run(JObject model, JObject repoInfo, string giteaToken)
        {
            _giteaHelper.SetToken(giteaToken);
            var localPath = _giteaHelper.CloneRepository(repoInfo["clone_url"].ToString(), repoInfo["name"].ToString());

            foreach (var id in model["app_ids"])
            {
                var connectionString = _configuration.GetConnectionString("StudioDBConnection");
                var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);
                var licenseKeyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "/devart.key";

                var connString = $"host={npgsqlConnection.Host};port={npgsqlConnection.Port};user id={npgsqlConnection.Username};password={npgsqlConnection.Password};database=app{id};license key=trial:" + licenseKeyPath;
                var connection = new PgSqlConnection(connString);

                connection.Open();
                var dumpConnection = new PgSqlDump { Connection = connection, Schema = "public", IncludeDrop = false };
                dumpConnection.Backup();
                var dump = dumpConnection.DumpText;
                connection.Close();

                var giteaDirectory = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);
                var localFolder = giteaDirectory + repoInfo["name"] + Path.DirectorySeparatorChar + "database";

                if (string.IsNullOrEmpty(dump))
                {
                    var notification = new Helpers.Email(_configuration, null, "app" + id + " Error", "app" + id + " has error");
                    notification.AddRecipient(model["notification_email"].ToString());
                    notification.AddToQueue("notifications@primeapps.io", "PrimeApps", null, null, "app" + id + " Error", "app" + id + " has error");

                    continue;
                }

                using (var fs = File.Create($"{localFolder}{Path.DirectorySeparatorChar}app{id}.sql"))
                {
                    var info = new UTF8Encoding(true).GetBytes(dump);
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                    //System.IO.File.WriteAllText (@"D:\path.txt", contents, Encoding.UTF8);
                }
            }

            using (var repo = new Repository(localPath))
            {
                //System.IO.File.WriteAllText(localPath, sample);
                Commands.Stage(repo, "*");

                var signature = new Signature(
                    new Identity("system", "system@primeapps.io"), DateTimeOffset.Now);

                var status = repo.RetrieveStatus();

                if (!status.IsDirty)
                {
                    _giteaHelper.DeleteDirectory(localPath);
                    throw new Exception("Unhandled exception. Repo status is dirty.");
                }

                // Commit to the repository
                var commit = repo.Commit("Database dump", signature, signature);
                _giteaHelper.Push(repo);

                repo.Dispose();
                _giteaHelper.DeleteDirectory(localPath);
            }

            if (!model["notification_email"].IsNullOrEmpty())
            {
                var notification = new Helpers.Email(_configuration, null, "Database Dump Ready", "Database dump is ready!");
                notification.AddRecipient(model["notification_email"].ToString());
                notification.AddToQueue("notifications@primeapps.io", "PrimeApps", null, null, "Database Dump Ready", "Database Dump Ready");
            }
        }
    }
}