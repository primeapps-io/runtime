using System.Threading.Tasks;
using PrimeApps.Model.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PrimeApps.App.Controllers
{
    [Route("api/tag"), Authorize]
    public class TagController : ApiBaseController
    {
        private ITagRepository _tagRepository;


        public TagController(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_tagRepository, DBMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("get_tag/{id:int}"), HttpGet]
        public async Task<IActionResult> GetTag(int id)
        {
            var tags = await _tagRepository.GetByFieldId(id);

            return Ok(tags);
        }
    }
}
