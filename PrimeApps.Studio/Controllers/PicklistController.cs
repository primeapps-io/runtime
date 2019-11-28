using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.Studio.Controllers
{
    [Route("api/picklist"), Authorize]
    public class PicklistController : DraftBaseController
    {
        private IPicklistRepository _picklistRepository;
        private IPermissionHelper _permissionHelper;

        public PicklistController(IPicklistRepository picklistRepository, IPermissionHelper permissionHelper)
        {
            _picklistRepository = picklistRepository;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_picklistRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.View))
                return StatusCode(403);

            var picklistEntity = await _picklistRepository.GetById(id);

            if (picklistEntity == null)
                return NotFound();

            var picklistViewModel = PicklistHelper.MapToViewModel(picklistEntity);

            return Ok(picklistViewModel);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.View))
                return StatusCode(403);

            var picklistEntities = await _picklistRepository.GetAll();

            return Ok(picklistEntities);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]List<int> ids)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.View))
                return StatusCode(403);

            var picklistEntities = await _picklistRepository.Find(ids);
            var picklistsViewModel = PicklistHelper.MapToViewModel(picklistEntities);

            return Ok(picklistsViewModel);
        }

        [Route("get_page")]
        public async Task<IActionResult> Find(ODataQueryOptions<Picklist> queryOptions)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.View))
                return StatusCode(403);
  
            var picklists = await _picklistRepository.Find();

            var queryResults = (IQueryable<Picklist>)queryOptions.ApplyTo(picklists);
            return Ok(new PageResult<Picklist>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));

        }

        [Route("get_item_page/{id:int}"), HttpPost]
        public async Task<IActionResult> GetItemPage(int id, [FromBody] PaginationModel paginationModel)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.View))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var itemList = await _picklistRepository.GetItemPage(id, paginationModel);

            return Ok(itemList);
        }

        [Route("count"), HttpGet]
        public async Task<int> Count()
        {
            var count = await _picklistRepository.Count();

            return count;
        }

        [Route("count_items/{id:int}"), HttpGet]
        public async Task<int> CountItems(int id)
        {
            var count = await _picklistRepository.CountItems(id);

            return count;
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]PicklistBindingModel picklist)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var picklistEntity = PicklistHelper.CreateEntity(picklist);
            var result = await _picklistRepository.Create(picklistEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
            return Ok(picklistEntity); //Created(uri.Scheme + "://" + uri.Authority + "/api/picklist/get/" + picklistEntity.Id, picklistEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/picklist/get/" + picklistEntity.Id, picklistEntity);
        }

        [Route("add_item/{id:int}"), HttpPost]
        public async Task<IActionResult> AddItem(int id, [FromBody] PicklistItemViewModel picklistItemModel)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isUnique = await _picklistRepository.GetItemUniqueBySystemCode(picklistItemModel.SystemCode);

            if (isUnique)
                return Conflict();

            var picklistItem = new PicklistItem
            {
                PicklistId = id,
                SystemCode = picklistItemModel.SystemCode,
                LabelEn = picklistItemModel.LabelTr,
                LabelTr = picklistItemModel.LabelTr,
                Value = picklistItemModel.Value,
                Value2 = picklistItemModel.Value2,
                Value3 = picklistItemModel.Value3,
                Order = picklistItemModel.Order,
                Inactive = false
            };

            var result = await _picklistRepository.AddItem(picklistItem);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            return Ok(result);
        }

        [Route("import/{id:int}"), HttpPost]
        public async Task<IActionResult> ImportItems(int id, [FromBody]List<PicklistItemViewModel> picklistItemsModel)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var picklist = await _picklistRepository.GetById(id);

            if (picklist == null)
                return NotFound();

            var orderCount = new short();

            if (picklist.Items.Count > 0)
                orderCount = picklist.Items.Max(x => x.Order);


            foreach (var item in picklistItemsModel)
            {
                var isUnique = await _picklistRepository.GetItemUniqueBySystemCode(item.SystemCode);

                if (isUnique)
                    return Conflict(new { name = item.Name, system_code = item.SystemCode, row = picklistItemsModel.IndexOf(item) });

                orderCount++;

                var itemEntity = new PicklistItem
                {
                    PicklistId = id,
                    LabelEn = item.Name,
                    LabelTr = item.Name,
                    SystemCode = item.SystemCode,
                    Value = item.Value,
                    Value2 = item.Value2,
                    Value3 = item.Value3,
                    Order = orderCount
                };

                picklist.Items.Add(itemEntity);
            }

            await _picklistRepository.Update(picklist);

            return Ok();
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]PicklistBindingModel picklist)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var picklistEntity = await _picklistRepository.GetById(id);

            if (picklistEntity == null)
                return NotFound();

            PicklistHelper.UpdateEntity(picklist, picklistEntity);
            await _picklistRepository.Update(picklistEntity);

            return Ok(picklistEntity);
        }

        [Route("update_item/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateItem(int id, [FromBody]PicklistItemBindingModel item)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var itemEntity = await _picklistRepository.GetItemById(id);

            if (itemEntity == null)
                return NotFound();

            PicklistHelper.UpdateItemEntity(item, itemEntity);
            var result = await _picklistRepository.Update(itemEntity);

            if (result >= 0)
                return Ok(itemEntity);
            else
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.Delete))
                return StatusCode(403);

            var picklistEntity = await _picklistRepository.GetById(id);

            if (picklistEntity == null)
                return NotFound();

            var result = await _picklistRepository.DeleteSoft(picklistEntity);

            return Ok(result);
        }

        [Route("delete_item/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteItem(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.Delete))
                return StatusCode(403);

            var picklistEntity = await _picklistRepository.GetItemById(id);

            if (picklistEntity == null)
                return NotFound();

            var result = await _picklistRepository.ItemDeleteSoft(picklistEntity);

            return Ok(result);
        }

        [Route("is_unique_check/{name}")]
        public async Task<IActionResult> isUniqueCheck(string name)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "picklist", RequestTypeEnum.View))
                return StatusCode(403);

            if (string.IsNullOrWhiteSpace(name))
                return NotFound();

            var result = await _picklistRepository.isUniqueCheck(name);

            return Ok(!result);
        }
    }
}
