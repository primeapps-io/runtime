using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Models;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using ModuleHelper = PrimeApps.App.Helpers.ModuleHelper;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.App.Controllers
{
    [Route("api/module"), Authorize, SnakeCase]
    public class ModuleController : BaseController
    {
        private IModuleRepository _moduleRepository;
        private IViewRepository _viewRepository;
        private IProfileRepository _profileRepository;
        private ISettingRepository _settingRepository;
        private Warehouse _warehouse;

        public ModuleController(IModuleRepository moduleRepository, IViewRepository viewRepository, IProfileRepository profileRepository, ISettingRepository settingRepository, Warehouse warehouse)
        {
            _moduleRepository = moduleRepository;
            _viewRepository = viewRepository;
            _profileRepository = profileRepository;
            _settingRepository = settingRepository;
            _warehouse = warehouse;
        }

        [Route("get_by_id/{id:int}"), HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var module = await _moduleRepository.GetById(id);

            if (module == null)
                return NotFound();

            return Ok(module);
        }

        [Route("get_by_name/{name:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpGet]
        public async Task<IActionResult> GetByName(string name)
        {
            var module = await _moduleRepository.GetByName(name);

            if (module == null)
                return NotFound();

            return Ok(module);
        }

        [Route("get_all"), HttpGet]
        public async Task<ICollection<Module>> GetAll()
        {
            return await _moduleRepository.GetAll();
        }

        [Route("get_all_deleted"), HttpGet]
        public async Task<ICollection<Module>> GetAllDeleted()
        {
            return await _moduleRepository.GetAllDeleted();
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create(ModuleBindingModel module)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Create module
            var moduleEntity = ModuleHelper.CreateEntity(module);
            var resultCreate = await _moduleRepository.Create(moduleEntity);

            if (resultCreate < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            //Create default views
            try
            {
                var defaultViewAllRecordsEntity = await ViewHelper.CreateDefaultViewAllRecords(moduleEntity, _moduleRepository);
                var defaultViewMyRecordsEntity = ViewHelper.CreateDefaultViewMyRecords(moduleEntity);

                var resultCreateViewAllRecords = await _viewRepository.Create(defaultViewAllRecordsEntity);

                if (resultCreateViewAllRecords < 1)
                {
                    await _moduleRepository.DeleteHard(moduleEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }

                var resultCreateViewMyRecords = await _viewRepository.Create(defaultViewMyRecordsEntity);

                if (resultCreateViewMyRecords < 1)
                {
                    await _moduleRepository.DeleteHard(moduleEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }
            }
            catch (Exception)
            {
                await _moduleRepository.DeleteHard(moduleEntity);
                throw;
            }

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            //Create dynamic table
            try
            {
                var resultCreateTable = await _moduleRepository.CreateTable(moduleEntity, AppUser.TenantLanguage);

                if (resultCreateTable != -1)
                {
                    await _moduleRepository.DeleteHard(moduleEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }
            }
            catch (Exception)
            {
                await _moduleRepository.DeleteHard(moduleEntity);
                throw;
            }

            //Create dynamic table indexes
            try
            {
                var resultCreateIndexes = await _moduleRepository.CreateIndexes(moduleEntity);

                if (resultCreateIndexes != -1)
                {
                    await _moduleRepository.DeleteTable(moduleEntity);
                    await _moduleRepository.DeleteHard(moduleEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }

            }
            catch (Exception)
            {
                await _moduleRepository.DeleteTable(moduleEntity);
                await _moduleRepository.DeleteHard(moduleEntity);
                throw;
            }

            //Create default permissions for the new module.
            await _profileRepository.AddModuleAsync(moduleEntity.Id);

            ModuleHelper.AfterCreate(AppUser, moduleEntity);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/module/get?id=" + moduleEntity.Id, moduleEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update([FromRoute]int id, [FromBody]ModuleBindingModel module)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetById(id);

            if (moduleEntity == null)
                return NotFound();

            //Update module
            var moduleChanges = ModuleHelper.UpdateEntity(module, moduleEntity);
            await _moduleRepository.Update(moduleEntity);

            //If there is no changes for dynamic tables then return ok
            if (moduleChanges == null)
                return Ok();

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            //Alter dynamic table
            try
            {
                var resultAlterTable = await _moduleRepository.AlterTable(moduleEntity, moduleChanges, AppUser.TenantLanguage);

                if (resultAlterTable != -1)
                {
                    var entityRevert = ModuleHelper.RevertEntity(moduleChanges, moduleEntity);
                    await _moduleRepository.Update(entityRevert);

                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                var entityRevert = ModuleHelper.RevertEntity(moduleChanges, moduleEntity);
                await _moduleRepository.Update(entityRevert);

                throw;
            }

            //Delete View Fields
            var views = await _viewRepository.GetAll(id);
            var fields = ModuleHelper.DeleteViewField(views, id, module.Fields);
            if (fields.Count > 0)
            {
                foreach (var field in fields)
                {
                    await _viewRepository.DeleteViewField(field.ViewId, field.Field);
                }
            }

            //var viewStates = await _viewRepository.GetAllViewStates(id);
            //if (viewStates.Count > 0)
            //{
            //    foreach (var viewState in viewStates)
            //    {
            //        await _viewRepository.DeleteHardViewState(viewState);
            //    }
            //}

            ModuleHelper.AfterUpdate(AppUser, moduleEntity);

            return Ok();
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var moduleEntity = await _moduleRepository.GetById(id);

            if (moduleEntity == null)
                return NotFound();

            var result = await _moduleRepository.DeleteSoft(moduleEntity);

            if (result > 0)
                ModuleHelper.AfterDelete(AppUser, moduleEntity);

            return Ok();
        }

        [Route("create_relation/{moduleId:int}"), HttpPost]
        public async Task<IActionResult> CreateRelation([FromRoute]int moduleId, [FromBody]RelationBindingModel relation)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetById(moduleId);

            if (moduleEntity == null)
                return NotFound();

            var currentRelations = moduleEntity.Relations.ToList();
            var relationEntity = ModuleHelper.CreateRelationEntity(relation, moduleEntity);
            var resultCreate = await _moduleRepository.CreateRelation(relationEntity);

            if (resultCreate < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            if (relationEntity.RelationType == RelationType.ManyToMany && !relation.TwoWay)
            {
                //Create dynamic junction table
                try
                {
                    var resultCreateJunctionTable = await _moduleRepository.CreateJunctionTable(moduleEntity, relationEntity, currentRelations);

                    if (resultCreateJunctionTable != -1)
                    {
                        await _moduleRepository.DeleteRelationHard(relationEntity);

                        throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

                        //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                    }
                }
                catch (Exception)
                {
                    await _moduleRepository.DeleteRelationHard(relationEntity);
                    throw;
                }
            }

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/module/get?id=" + moduleEntity.Id, moduleEntity);
        }

        [Route("update_relation/{moduleId:int}/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateRelation([FromRoute]int moduleId, [FromRoute]int id, [FromBody]RelationBindingModel relation)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetById(moduleId);

            if (moduleEntity == null)
                return NotFound();

            var relationEntity = await _moduleRepository.GetRelation(id);

            if (relationEntity == null)
                return NotFound();

            ModuleHelper.UpdateRelationEntity(relation, relationEntity, moduleEntity);
            await _moduleRepository.UpdateRelation(relationEntity);

            return Ok();
        }

        [Route("delete_relation/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteRelation([FromRoute]int id)
        {
            var relationEntity = await _moduleRepository.GetRelation(id);

            if (relationEntity == null)
                return NotFound();

            await _moduleRepository.DeleteRelationSoft(relationEntity);

            return Ok();
        }

        [Route("create_dependency/{moduleId:int}"), HttpPost]
        public async Task<IActionResult> CreateDependency([FromRoute]int moduleId, [FromBody]DependencyBindingModel dependency)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetById(moduleId);

            if (moduleEntity == null)
                return NotFound();

            var dependencyEntity = ModuleHelper.CreateDependencyEntity(dependency, moduleEntity);
            var resultCreate = await _moduleRepository.CreateDependency(dependencyEntity);

            if (resultCreate < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/module/get?id=" + moduleEntity.Id, moduleEntity);
        }

        [Route("update_dependency/{moduleId:int}/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateDependency([FromRoute]int moduleId, [FromRoute]int id, [FromBody]DependencyBindingModel dependency)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetById(moduleId);

            if (moduleEntity == null)
                return NotFound();

            var dependencyEntity = await _moduleRepository.GetDependency(id);

            if (dependencyEntity == null)
                return NotFound();

            ModuleHelper.UpdateDependencyEntity(dependency, dependencyEntity, moduleEntity);
            await _moduleRepository.UpdateDependency(dependencyEntity);

            return Ok();
        }

        [Route("update_field/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateField([FromRoute]int id, [FromBody]FieldBindingModel field)
        {
            var fieldEntity = await _moduleRepository.GetField(id);

            if (fieldEntity == null)
                return NotFound();
  
            fieldEntity.InlineEdit = field.InlineEdit;

            await _moduleRepository.UpdateField(fieldEntity);

            return Ok();
        }
        [Route("delete_dependency/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteDependency([FromRoute]int id)
        {
            var dependencyEntity = await _moduleRepository.GetDependency(id);

            if (dependencyEntity == null)
                return NotFound();

            await _moduleRepository.DeleteDependencySoft(dependencyEntity);

            return Ok();
        }

        [Route("get_module_settings"), HttpGet]
        public async Task<IActionResult> GetModuleSettings()
        {
            var moduleSettings = await _settingRepository.GetAsync(SettingType.Module);

            return Ok(moduleSettings);
        }
    }
}
