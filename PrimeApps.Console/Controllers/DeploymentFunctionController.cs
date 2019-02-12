using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Console.Helpers;
using PrimeApps.Console.Models;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;

namespace PrimeApps.Console.Controllers
{
    [Route("api/deployment_function")]
    public class DeploymentFunctionController : DraftBaseController
    {
        private IConfiguration _configuration;
        private IDeploymentFunctionRepository _deploymentFunctionRepository;

        public DeploymentFunctionController(IConfiguration configuration, IDeploymentFunctionRepository deploymentFunctionRepository)
        {
            _configuration = configuration;
            _deploymentFunctionRepository = deploymentFunctionRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_deploymentFunctionRepository, PreviewMode, AppId, TenantId);
            base.OnActionExecuting(context);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _deploymentFunctionRepository.Count();

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            var deployments = await _deploymentFunctionRepository.Find(paginationModel); ;

            return Ok(deployments);
        }

        [Route("get/{id}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var deployment = await _deploymentFunctionRepository.Get(id);

            if (deployment == null)
                return BadRequest();

            return Ok(deployment);
        }
        
        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]DeploymentFunctionBindingModel deployment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var deploymentObj = new DeploymentFunction()
            {
                Version = "1",
                StartTime = DateTime.Now,
                Status = DeploymentStatus.Running
            };

            var createResult = await _deploymentFunctionRepository.Create(deploymentObj);

            if (createResult < 0)
                return BadRequest("An error occurred while creating an function");

            return Ok(deploymentObj);
        }

        [Route("update/{id}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]DeploymentFunctionBindingModel deployment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var deploymentObj = await _deploymentFunctionRepository.Get(id);

            if (deployment == null)
                return BadRequest("Function deployment not found.");

            deploymentObj.Status = deployment.Status;
            deploymentObj.Version = deployment.Version;
            deploymentObj.StartTime = deployment.StartTime;
            deploymentObj.EndTime = deployment.EndTime;

            var result = await _deploymentFunctionRepository.Update(deploymentObj);

            if (result < 0)
                return BadRequest("An error occurred while update function deployment.");
  
            return Ok(result);
        }

        [Route("delete/{id}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var function = await _deploymentFunctionRepository.Get(id);

            if (function == null)
                return BadRequest();

            var result = await _deploymentFunctionRepository.Delete(function);

            if (result < 0)
                return BadRequest("An error occurred while deleting an function deployment.");

            return Ok(result);
        }
    }
}
