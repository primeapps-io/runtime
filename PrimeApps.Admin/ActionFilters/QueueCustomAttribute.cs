using System;
using Hangfire.Common;
using Hangfire.States;
using Humanizer;

namespace PrimeApps.Admin.ActionFilters
{
    public class QueueCustomAttribute : JobFilterAttribute, IElectStateFilter
    {
        public void OnStateElection(ElectStateContext context)
        {
            var enqueuedState = context.CandidateState as EnqueuedState;
            if (enqueuedState != null)
            {
                enqueuedState.Queue = "queue_" + Environment.MachineName.Underscore();
            }
        }
    }
}