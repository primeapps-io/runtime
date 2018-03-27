using Hangfire.Common;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PrimeApps.App.Jobs.QueueAttributes
{
    public class MessagingQueueAttribute : JobFilterAttribute, IElectStateFilter
    {
        public String Queue { get; }

        public MessagingQueueAttribute()
        {
            Queue = ConfigurationManager.AppSettings["HangfireMessagingQueue"];
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