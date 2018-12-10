using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using RecordHelper = PrimeApps.App.Helpers.RecordHelper;

namespace PrimeApps.App.Bpm.Steps
{
    public class TaskStep : StepBodyAsync
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IConfiguration _configuration;

        public string Request { get; set; }

        public string Response { get; set; }

        public TaskStep(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            if (context == null)
                throw new NullReferenceException();

            if (context.Workflow.Reference == null)
                throw new NullReferenceException();

            var appUser = JsonConvert.DeserializeObject<UserItem>(context.Workflow.Reference);
            var _currentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id };

            var newRequest = Request != null ? JObject.Parse(Request.Replace("\\", "")) : null;

            if (newRequest.IsNullOrEmpty())
                throw new Exception("Cannot find Request");

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();


                using (var _platformWarehouseRepository = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
                using (var _analyticRepository = new AnalyticRepository(databaseContext, _configuration))
                {
                    _platformWarehouseRepository.CurrentUser = _analyticRepository.CurrentUser = _currentUser;

                    var warehouse = new Model.Helpers.Warehouse(_analyticRepository, _configuration);
                    var warehouseEntity = await _platformWarehouseRepository.GetByTenantId(_currentUser.TenantId);

                    if (warehouseEntity != null)
                        warehouse.DatabaseName = warehouseEntity.DatabaseName;
                    else
                        warehouse.DatabaseName = "0";

                    using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                    using (var _recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
                    using (var _recordHelper = new RecordHelper(_configuration, _serviceScopeFactory, _currentUser))
                    {
                        _moduleRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser;

                        var data = JObject.FromObject(context.Workflow.Data);

                        if (newRequest.IsNullOrEmpty() || data["module_id"].IsNullOrEmpty())
                            throw new MissingFieldException("Cannot find child data");

                        var createTask = newRequest["create_task"];
                        var moduleId = data["module_id"].ToObject<int>();
                        var module = await _moduleRepository.GetById(moduleId);
                        var record = data["record"].ToObject<JObject>();

                        var moduleActivity = await _moduleRepository.GetByName("activities");

                        var task = new JObject();
                        task["activity_type"] = "1";
                        task["owner"] = createTask["owner"];
                        task["subject"] = createTask["subject"];

                        if (module.Name == "activities")
                        {
                            task["related_module"] = 900000 + module.Id;
                            task["related_to"] = (int)record["id"];
                        }
                        else if (!record["related_module"].IsNullOrEmpty())
                        {
                            var moduleCurrent = await _moduleRepository.GetByLabel((string)record["related_module"]);

                            if (moduleCurrent == null)
                                throw new NullReferenceException();

                            task["related_module"] = 900000 + moduleCurrent.Id;
                            task["related_to"] = record["related_to"];
                        }

                        var dueDateValue = createTask["task_due_date"]["value"].ToString() == "now" ? 0 : createTask["task_due_date"]["value"].Value<double>();
                        task["task_due_date"] = DateTime.UtcNow.AddDays(dueDateValue);

                        if (!createTask["task_status"].IsNullOrEmpty())
                            task["task_status"] = createTask["task_status"]["id"];

                        if (!createTask["task_priority"].IsNullOrEmpty())
                            task["task_priority"] = createTask["task_priority"]["id"];

                        if (!createTask["task_notification"].IsNullOrEmpty())
                            task["task_notification"] = createTask["task_notification"]["id"];

                        if (!createTask["task_reminder"].IsNullOrEmpty())
                            task["task_reminder"] = createTask["task_reminder"]["id"];

                        if (!createTask["reminder_recurrence"].IsNullOrEmpty())
                            task["reminder_recurrence"] = createTask["reminder_recurrence"]["id"];

                        if (!string.IsNullOrWhiteSpace(createTask["description"].Value<string>()))
                            task["description"] = createTask["description"];

                        if (record["owner"].IsNullOrEmpty())
                            task["owner"] = !record["owner"].IsNullOrEmpty() ? (int)record["owner"] : (int)record["owner.id"];

                        task["created_by"] = createTask["created_by_id"];

                        var modelState = new ModelStateDictionary();
                        var resultBefore = await _recordHelper.BeforeCreateUpdate(moduleActivity, task, modelState, appUser.Language);

                        if (resultBefore < 0 && !modelState.IsValid)
                        {
                            throw new OperationCanceledException("Record cannot be created! Object: " + task + " ModelState: " + modelState.ToJsonString());
                        }

                        var resultCreate = await _recordRepository.Create(task, moduleActivity);

                        if (resultCreate < 1)
                        {
                            throw new OperationCanceledException("Record cannot be created! Object: " + task);
                        }

                        _recordHelper.AfterCreate(moduleActivity, task, appUser, warehouse, moduleActivity.Id != module.Id);

                    }
                }
            }

            return ExecutionResult.Next();
        }
    }
}
