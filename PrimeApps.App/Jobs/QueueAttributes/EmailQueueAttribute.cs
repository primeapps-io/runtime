using Hangfire.Common;
using Hangfire.States;
using System;
using System.Configuration;
using Microsoft.IdentityModel.Protocols;

namespace PrimeApps.App.Jobs.QueueAttributes
{
    public class EmailQueueAttribute : JobFilterAttribute, IElectStateFilter
    {
        public String Queue { get; }

        public EmailQueueAttribute()
        {
            Queue = ConfigurationManager.AppSettings["HangfireEmailQueue"];
        }
        public void OnStateElection(ElectStateContext context)
        {
            var enqueuedState = context.CandidateState as EnqueuedState;
            if (enqueuedState != null)
            {
                enqueuedState.Queue = Queue;
            }
        }
    }
}