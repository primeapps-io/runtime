using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Bpm.Steps
{
	public class DataReadStep : StepBodyAsync
	{
		private IServiceScopeFactory _serviceScopeFactory;
		private IConfiguration _configuration;

		public string Request { get; set; }
		public string Response { get; set; }

		public DataReadStep(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
		{
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
		}

		public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

			if (context == null)
				throw new NullReferenceException();

			if (context.Workflow.Reference == null)
				throw new NullReferenceException();

			var appUser = JsonConvert.DeserializeObject<UserItem>(context.Workflow.Reference);
			var currentUser = new CurrentUser { };

			currentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };

			var request = Request != null ? JsonConvert.DeserializeObject<JObject>(Request.Replace("\\", "")) : null;
			var moduleId = 0;
			var recordId = 0;
			var recordKey = "id";

			if (!request.IsNullOrEmpty())
			{
				var dataReadRequest = request["data_read"];
				moduleId = !dataReadRequest["module_id"].IsNullOrEmpty() ? (int)dataReadRequest["module_id"] : 0;
				recordId = !dataReadRequest["record_id"].IsNullOrEmpty() ? (int)dataReadRequest["record_id"] : 0;
				recordKey = (string)dataReadRequest["record_key"];
			}

			if (moduleId < 1 || recordId < 1)
			{
				var tempData = JObject.FromObject(context.Workflow.Data);

				moduleId = tempData["module_id"].Value<int>();
				recordId = tempData["record"]["id"].Value<int>();
			}

			JObject record;

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();

				using (var moduleRepository = new ModuleRepository(databaseContext, _configuration))
				using (var recordRepository = new RecordRepository(databaseContext, null, _configuration))
				{
					moduleRepository.CurrentUser = recordRepository.CurrentUser = currentUser;
					var module = await moduleRepository.GetById(moduleId);
					var lookupModules = await RecordHelper.GetLookupModules(module, moduleRepository, tenantLanguage: appUser.TenantLanguage);
					record = await recordRepository.GetById(module, recordId, true, lookupModules, operation: OperationType.read);
				}
			}

			var data = new BpmReadDataModel();
			data.ConditionValue = (string)record[recordKey];
			data.record = record;
			Response = data.ConditionValue;

			return ExecutionResult.Outcome(data);
		}
	}
}
