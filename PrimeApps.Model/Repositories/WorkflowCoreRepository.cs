using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories
{
    public class WorkflowCoreRepository : RepositoryBasePlatform, IWorkflowCoreRepository
    {
        public WorkflowCoreRepository(PlatformDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public JArray GetWorkflowInstances(string code)
        {
            var sql = "SELECT wc.*\n" +
                      "FROM wfc.\"Workflow\" wc\n" +
                      $"WHERE wc.\"WorkflowDefinitionId\" = '{code}'\n" +
                      "ORDER BY wc.\"CreateTime\"";

            var workflowInstances = DbContext.Database.SqlQueryDynamic(sql);

            return workflowInstances;
        }

        public JArray GetExecutionPointers(string code)
        {
            var sql = "SELECT ep.*\n" +
                      "FROM wfc.\"Workflow\" wc\n" +
                      "JOIN wfc.\"ExecutionPointer\" ep ON ep.\"WorkflowId\" = wc.\"PersistenceId\"\n" +
                      $"WHERE wc.\"WorkflowDefinitionId\" = '{code}'\n" +
                      "ORDER BY ep.\"StepId\"";

            var executionPointers = DbContext.Database.SqlQueryDynamic(sql);

            return executionPointers;
        }
    }
}
