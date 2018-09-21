using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Application;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    public static class BpmHelper
    {
        public static async Task<BpmWorkflow> CreateEntity(BpmWorkflowBindingModel bpmWorkflowModel)
        {
            var bpmWorkflow = new BpmWorkflow
            {
                Name = bpmWorkflowModel.Name,
                DefinitionJson = bpmWorkflowModel.DefinitionJson.ToJsonString()
            //other fields to be added
        };

            return bpmWorkflow;
        }

        public static async Task<BpmWorkflow> UpdateEntity(BpmWorkflowBindingModel bpmWorkflowModel, BpmWorkflow bpmWorkflow)
        {
            bpmWorkflow.Name = bpmWorkflowModel.Name;
            bpmWorkflow.DefinitionJson = bpmWorkflowModel.DefinitionJson.ToJsonString();

            return bpmWorkflow;
        }
    }
}