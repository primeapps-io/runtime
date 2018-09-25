using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.App.Models;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    public interface IBpmHelper
    {
        Task<BpmWorkflow> CreateEntity(BpmWorkflowBindingModel bpmWorkflowModel);

        Task<BpmWorkflow> UpdateEntity(BpmWorkflowBindingModel bpmWorkflowModel, BpmWorkflow bpmWorkflow);

    }
    public class BpmHelper:IBpmHelper
    {
        private CurrentUser _currentUser;
        private IHttpContextAccessor _context;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        public BpmHelper(IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor context, IConfiguration configuration)
        {
            _context = context;
            _currentUser = UserHelper.GetCurrentUser(_context);
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }

        public async Task<BpmWorkflow> CreateEntity(BpmWorkflowBindingModel bpmWorkflowModel)
        {
            var bpmWorkflow = new BpmWorkflow
            {
                Name = bpmWorkflowModel.Name,
                Description = bpmWorkflowModel.Description,
                CategoryId = bpmWorkflowModel.CategoryId,
                StartTime = bpmWorkflowModel.StartTime,
                EndTime = bpmWorkflowModel.EndTime,
                TriggerType = bpmWorkflowModel.TriggerType,
                RecordOperations = bpmWorkflowModel.RecordOperations,
                Frequency = bpmWorkflowModel.Frequency,
                ChangedFieldsArray = bpmWorkflowModel.ChangedFields,
                CanStartManuel = bpmWorkflowModel.CanStartManuel,
                DefinitionJson = bpmWorkflowModel.DefinitionJson.ToJsonString(),
                DiagramJson = bpmWorkflowModel.DiagramJson
            };

            using(var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
               
            }

            //if (bpmWorkflowModel.CategoryId != null && bpmWorkflowModel.CategoryId > 0)
            //{

            //}

            return bpmWorkflow;
        }

        public async Task<BpmWorkflow> UpdateEntity(BpmWorkflowBindingModel bpmWorkflowModel, BpmWorkflow bpmWorkflow)
        {
            bpmWorkflow.Name = bpmWorkflowModel.Name;
            bpmWorkflow.DefinitionJson = bpmWorkflowModel.DefinitionJson.ToJsonString();

            return bpmWorkflow;
        }


    }
}