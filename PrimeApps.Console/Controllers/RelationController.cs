using PrimeApps.Console.Models;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq;
using PrimeApps.Model.Common;

namespace PrimeApps.Console.Controllers
{
    [Route("api/relation")]
    public class RelationController : DraftBaseController
    {
        private IRelationRepository _relationRepository;
        private IProfileRepository _profileRepository;
        private ISettingRepository _settingRepository;
        private IModuleRepository _moduleRepository;
        private IConfiguration _configuration;
        private Warehouse _warehouse;

        private IModuleHelper _moduleHelper;

        public RelationController(IRelationRepository relationRepository, IProfileRepository profileRepository, ISettingRepository settingRepository, IModuleRepository moduleRepository, Warehouse warehouse, IModuleHelper moduleHelper, IConfiguration configuration)
        {
            _relationRepository = relationRepository;
            _profileRepository = profileRepository;
            _settingRepository = settingRepository;
            _moduleRepository = moduleRepository;
            _warehouse = warehouse;
            _configuration = configuration;
            _moduleHelper = moduleHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_relationRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_profileRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_settingRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_moduleRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _relationRepository.Count();

            if (count == null)
                return NotFound();

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            var relations = await _relationRepository.Find(paginationModel);

            if (relations == null)
                return NotFound();

            foreach (var relation in relations)
            {
                var relationModule = await _moduleRepository.GetBasicByName(relation.RelatedModule);
                relation.RelationModule = relationModule;
            }

            return Ok(relations);
        }

        [Route("get_all"), HttpGet]
        public async Task<ICollection<Relation>> GetAll()
        {
            return await _relationRepository.GetAll();
        }

        [Route("get_all_deleted"), HttpGet]
        public async Task<ICollection<Relation>> GetAllDeleted()
        {
            return await _relationRepository.GetAllDeleted();
        }

    }
}
