using System;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DiagnosticAdapter;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;

namespace PrimeApps.Studio.Services
{
    public class CommandListener
    {
        private IBackgroundTaskQueue _queue;
        private IApplicationBuilder _app;

        public CommandListener(IBackgroundTaskQueue queue, IApplicationBuilder app)
        {
            _queue = queue;
            _app = app;
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
        public void OnCommandExecuting(DbCommand command, DbCommandMethod executeMethod, Guid commandId, Guid connectionId, bool async, DateTimeOffset startTime)
        {
            if (command.CommandText.ToUpper().StartsWith("INSERT") || command.CommandText.ToUpper().StartsWith("UPDATE") || command.CommandText.ToUpper().StartsWith("DELETE"))
            {
                _queue.QueueBackgroundWorkItem(token =>
                {
                    using (var scope = _app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                    {
                        var _configuration = _app.ApplicationServices.GetService<IConfiguration>();

                        var databaseContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();
    
                        using (var organizationRepository = new OrganizationRepository(databaseContext, _configuration))
                        {
                            var check = organizationRepository.IsOrganizationAvaliable(637, 276);
                        }
                    }

                    return null;
                });


                Console.WriteLine("OnCommandExecuting");
            }
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")]
        public void OnCommandExecuted(object result, bool async)
        {
            Console.WriteLine("OnCommandExecuted");
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandError")]
        public void OnCommandError(Exception exception, bool async)
        {
            Console.WriteLine("OnCommandError");
        }
    }
}