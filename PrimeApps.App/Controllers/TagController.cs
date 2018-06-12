using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using OfisimCRM.App.ActionFilters;
using OfisimCRM.Model.Enums;
using OfisimCRM.Model.Repositories.Interfaces;
using OfisimCRM.App.Models;
using OfisimCRM.App.Helpers;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.Controllers;

namespace OfisimCRM.App.Controllers
{
    [RoutePrefix("api/tag"), Authorize, SnakeCase]
    public class TagController : BaseController
    {
        private ITagRepository _tagRepository;


        public TagController(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;

        }

        [Route("get_tag/{id:int}"), HttpGet]
        public async Task<IHttpActionResult> GetTag(int id)
        {
            var tags = await _tagRepository.GetByFieldId(id);

            return Ok(tags);
        }
    }
}
