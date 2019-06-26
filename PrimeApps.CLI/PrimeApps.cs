using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PrimeApps.CLI
{
    [Command(Name = "primeapps", Description = "PrimeApps CLI"),
                 Subcommand(typeof(Login.Storage)),
                 Subcommand(typeof(List.List))]
    [HelpOption]
    class PrimeApps
    {
        private static ILoggerFactory loggerFactory;
        public static IConfigurationRoot Configuration;
        private readonly IConsole _console;

        public static ILoggerFactory LoggerFactory { get => loggerFactory; set => loggerFactory = value; }

        static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<PrimeApps>(args);

        public PrimeApps(IConsole console)
        {
            _console = console;
            // Adding JSON file into IConfiguration.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // Setup DI
            var serviceProvider = new ServiceCollection()
                .AddLogging(config => config.SetMinimumLevel(LogLevel.Error))
                .AddEntityFrameworkNpgsql()
                .AddSingleton<IConfiguration>(Configuration)
                .BuildServiceProvider();

            LoggerFactory = serviceProvider.GetService<ILoggerFactory>();

            var logger = LoggerFactory.CreateLogger<PrimeApps>();
            logger.LogDebug("Starting application");
        }

        private int OnExecute(CommandLineApplication app)
        {
            _console.WriteLine("You must specify at least a subcommand.");
            app.ShowHelp();
            return 1;
        }
    }
}