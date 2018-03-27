using System.Configuration;
using Hangfire.Common;
using Hangfire.States;

namespace PrimeApps.Model.Helpers
{
    public class WarehouseQueueAttribute : JobFilterAttribute, IElectStateFilter
    {
        /// <summary>
        /// Gets the queue name that will be used for background jobs.
        /// </summary>
        public string Queue { get; }

        /// <summary>
        /// Initializes a new instance using the HangfireQueueName app setting in web.config.
        /// </summary>
        public WarehouseQueueAttribute()
        {
            Queue = ConfigurationManager.AppSettings["HangfireWarehouseQueue"];
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