using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories
{
    public class WorkflowCoreRepository : RepositoryBasePlatform, IWorkflowCoreRepository
    {
        public WorkflowCoreRepository(PlatformDBContext dbContext, IConfiguration configuration, ICacheHelper cacheHelper) : base(dbContext, configuration, cacheHelper) { }

        public JArray GetWorkflowInstances(string code)
        {
            var sql = "SELECT wc.*\n" +
                      "FROM wfc.\"Workflow\" wc\n" +
                      $"WHERE wc.\"WorkflowDefinitionId\" = '{code}'\n" +
                      "ORDER BY wc.\"CreateTime\"";

            var workflowInstances = DbContext.Database.SqlQueryDynamic(sql);

            return workflowInstances;
        }

        public JArray GetExecutionPointers(int workflowInstanceId)
        {
            var sql = "SELECT ep.*\n" +
                      "FROM wfc.\"ExecutionPointer\" ep\n" +
                      $"WHERE ep.\"WorkflowId\" = '{workflowInstanceId}'\n" +
                      "ORDER BY ep.\"StepId\"";

            var executionPointers = DbContext.Database.SqlQueryDynamic(sql);

            return executionPointers;
        }
    }
}
