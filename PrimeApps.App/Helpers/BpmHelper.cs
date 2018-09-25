using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    public interface IBpmHelper
    {
        Task<BpmWorkflow> CreateEntity(BpmWorkflowBindingModel bpmWorkflowModel, string tenantLanguage);

        Task UpdateEntity(BpmWorkflowBindingModel bpmWorkflowModel, BpmWorkflow bpmWorkflow, string tenantLanguage);

        string ReferanceCreateToForBpmHost(UserItem appUser);
    }
    public class BpmHelper : IBpmHelper
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

        public async Task<BpmWorkflow> CreateEntity(BpmWorkflowBindingModel bpmWorkflowModel, string tenantLanguage)
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
                ChangedFields = bpmWorkflowModel.ChangedFields,
                CanStartManuel = bpmWorkflowModel.CanStartManuel,
                DefinitionJson = bpmWorkflowModel.DefinitionJson.ToJsonString(),
                DiagramJson = bpmWorkflowModel.DiagramJson,
                ModuleId = bpmWorkflowModel.ModuleId
            };

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                using (var _categoryRepository = new BpmCategoryRepository(databaseContext, _configuration))
                {
                    _moduleRepository.CurrentUser = _picklistRepository.CurrentUser = _categoryRepository.CurrentUser = _currentUser;

                    //workflow's category is pulling
                    if (bpmWorkflow.CategoryId != null && bpmWorkflow.CategoryId > 0)
                    {
                        var category = await _categoryRepository.Get((int)bpmWorkflow.CategoryId);

                        bpmWorkflow.Category = category;
                    }

                    //workflow's Filters is pulling
                    if (bpmWorkflowModel.Filters != null && bpmWorkflowModel.Filters.Count > 0)
                    {
                        var module = await _moduleRepository.GetById(bpmWorkflowModel.ModuleId);
                        var picklistItemsIDs = new List<int>();
                        bpmWorkflowModel.Filters = new List<BpmRecordFilter>();

                        foreach (var filter in bpmWorkflowModel.Filters)
                        {
                            var field = module.Fields.Single(x => x.Name == filter.Field);

                            int tempvalue;
                            if (field.DataType == DataType.Picklist)
                            {
                                if (int.TryParse(filter.Value.ToString(), out tempvalue))
                                {
                                    picklistItemsIDs.Add(int.Parse(filter.Value.ToString()));
                                }
                            }
                            else if (field.DataType == DataType.Multiselect)
                            {
                                var values = filter.Value.ToString().Split(',');

                                foreach (var value in values)
                                {
                                    picklistItemsIDs.Add(int.Parse(value));
                                }
                            }
                        }

                        ICollection<PicklistItem> picklistItems = null;

                        if (picklistItemsIDs.Count > 0)
                            picklistItems = await _picklistRepository.FindItems(picklistItemsIDs);

                        foreach (var filter in bpmWorkflowModel.Filters)
                        {
                            var field = module.Fields.Single(x => x.Name == filter.Field);
                            var value = filter.Value.ToString();

                            if (filter.Operator != Operator.Empty && filter.Operator != Operator.NotEmpty)
                            {
                                if (field.DataType == DataType.Picklist)
                                {
                                    var picklistItem = picklistItems.Single(x => x.Id == int.Parse(filter.Value.ToString()));
                                    value = tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn;
                                }
                                else if (field.DataType == DataType.Multiselect)
                                {
                                    var picklistLabels = new List<string>();
                                    var values = filter.Value.ToString().Split(',');

                                    foreach (var item in values)
                                    {
                                        var picklistItem = picklistItems.Single(q => q.Id == int.Parse(item));
                                        picklistLabels.Add(tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn);
                                    }

                                    value = string.Join("|", picklistLabels);
                                }
                                else if (field.DataType == DataType.Tag)
                                { }
                            }

                            var recordFilter = new BpmRecordFilter
                            {
                                Field = filter.Field,
                                Operator = filter.Operator,
                                Value = value,
                                No = filter.No
                            };

                            bpmWorkflow.Filters.Add(recordFilter);
                        }
                    }
                }
            }

            return bpmWorkflow;
        }

        public async Task UpdateEntity(BpmWorkflowBindingModel bpmWorkflowModel, BpmWorkflow bpmWorkflow, string tenantLanguage)
        {
            bpmWorkflow.Name = bpmWorkflowModel.Name;
            bpmWorkflow.Description = bpmWorkflowModel.Description;
            bpmWorkflow.CategoryId = bpmWorkflowModel.CategoryId;
            bpmWorkflow.StartTime = bpmWorkflowModel.StartTime;
            bpmWorkflow.EndTime = bpmWorkflowModel.EndTime;
            bpmWorkflow.TriggerType = bpmWorkflowModel.TriggerType;
            bpmWorkflow.RecordOperations = bpmWorkflowModel.RecordOperations;
            bpmWorkflow.Frequency = bpmWorkflowModel.Frequency;
            bpmWorkflow.ChangedFields = bpmWorkflowModel.ChangedFields;
            bpmWorkflow.CanStartManuel = bpmWorkflowModel.CanStartManuel;
            bpmWorkflow.DefinitionJson = bpmWorkflowModel.DefinitionJson.ToJsonString();
            bpmWorkflow.DiagramJson = bpmWorkflowModel.DiagramJson;
            bpmWorkflow.ModuleId = bpmWorkflowModel.ModuleId;

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                using (var _categoryRepository = new BpmCategoryRepository(databaseContext, _configuration))
                {
                    _moduleRepository.CurrentUser = _picklistRepository.CurrentUser = _categoryRepository.CurrentUser = _currentUser;

                    if (bpmWorkflow.CategoryId != null && bpmWorkflow.CategoryId > 0)
                    {
                        var category = await _categoryRepository.Get((int)bpmWorkflow.CategoryId);

                        bpmWorkflow.Category = category;
                    }

                    if (bpmWorkflowModel.Filters != null && bpmWorkflowModel.Filters.Count > 0)
                    {
                        if (bpmWorkflow.Filters == null)
                            bpmWorkflow.Filters = new List<BpmRecordFilter>();

                        var module = await _moduleRepository.GetById(bpmWorkflowModel.ModuleId);
                        var picklistItemIds = new List<int>();

                        foreach (var filter in bpmWorkflowModel.Filters)
                        {
                            if (filter.Operator != Operator.Empty && filter.Operator != Operator.NotEmpty)
                            {
                                var field = module.Fields.Single(q => q.Name == filter.Field);

                                if (field.DataType == DataType.Picklist)
                                {
                                    picklistItemIds.Add(int.Parse(filter.Value.ToString()));
                                }
                                else if (field.DataType == DataType.Multiselect)
                                {
                                    var values = filter.Value.ToString().Split(',');

                                    foreach (var item in values)
                                    {
                                        picklistItemIds.Add(int.Parse(item));
                                    }
                                }
                            }
                        }

                        ICollection<PicklistItem> picklistItems = null;

                        if (picklistItemIds.Count > 0)
                            picklistItems = await _picklistRepository.FindItems(picklistItemIds);

                        foreach (var filter in bpmWorkflowModel.Filters)
                        {
                            var field = module.Fields.Single(x => x.Name == filter.Field);
                            var value = filter.Value.ToString();

                            if (filter.Operator != Operator.Empty && filter.Operator != Operator.NotEmpty)
                            {
                                if (field.DataType == DataType.Picklist)
                                {
                                    var picklistItem = picklistItems.Single(x => x.Id == int.Parse(filter.Value.ToString()));
                                    value = tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn;
                                }
                                else if (field.DataType == DataType.Multiselect)
                                {
                                    var picklistLabels = new List<string>();

                                    var values = filter.Value.ToString().Split(',');

                                    foreach (var val in values)
                                    {
                                        var picklistItem = picklistItems.Single(x => x.Id == int.Parse(val));
                                        picklistLabels.Add(tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn);
                                    }

                                    value = string.Join("|", picklistLabels);
                                }
                            }

                            var recordFilter = new BpmRecordFilter
                            {
                                Field = filter.Field,
                                Operator = filter.Operator,
                                Value = value,
                                No = filter.No
                            };

                            bpmWorkflow.Filters.Add(recordFilter);
                        }
                    }
                } 
            }


        }

        /// <summary>
        ///  Created to generate Reference data to know which user Tenants to BpmWorkflows
        /// </summary>
        /// <param name="appUser"></param>
        /// <returns></returns>
        public string ReferanceCreateToForBpmHost(UserItem appUser)
        {
            var referance = new JObject();

            referance["user_id"] = appUser.ProfileId;
            referance["tenant_id"] = appUser.TenantId;

            return referance.ToString();
        }
    }

}