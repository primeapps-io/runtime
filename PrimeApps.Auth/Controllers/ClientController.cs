using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrimeApps.Auth.Models.Api.ClientApiModels;
using PrimeApps.Auth.Repositories.IRepositories;
using PrimeApps.Model.Context;

namespace PrimeApps.Auth.Controllers
{
    [Route("[controller]")]
    public class ClientController : ApiBaseController<ClientController>
    {
        private IClientRepository _clientRepository;

        public ClientController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody]CreateClientBindingModel client)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newClient = new Client
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                AllowedGrantTypes = client.AllowedGrantTypes.Split(";"),
                AllowRememberConsent = false,
                AlwaysSendClientClaims = true,
                RequireConsent = false,

                ClientSecrets =
                {
                    new Secret(client.ClientSecrets.Sha256())
                },

                RedirectUris = client.RedirectUris.Split(";"),
                PostLogoutRedirectUris = client.PostLogoutRedirectUris.Split(";"),
                AllowedScopes = client.AllowedScopes.Split(";"),
                AccessTokenLifetime = client.AccessTokenLifetime
            };

            var result = await _clientRepository.Create(newClient.ToEntity());

            if (result < 1)
                return BadRequest();

            return Ok();
        }
        
        [Route("update/{clientId}"), HttpPut]
        public async Task<IActionResult> Update(string clientId, [FromBody]CreateClientBindingModel clientModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = await _clientRepository.Get(clientId);

            if (client == null)
                return NotFound();
            
            var newClient = new Client
            {
                ClientName = clientModel.ClientName,
                AllowedGrantTypes = clientModel.AllowedGrantTypes.Split(";"),
                AllowRememberConsent = false,
                AlwaysSendClientClaims = true,
                RequireConsent = false,
                RedirectUris = clientModel.RedirectUris.Split(";"),
                PostLogoutRedirectUris = clientModel.PostLogoutRedirectUris.Split(";"),

                AllowedScopes = clientModel.AllowedScopes.Split(";"),
                AccessTokenLifetime = clientModel.AccessTokenLifetime
            };

            var result = await _clientRepository.Update(newClient.ToEntity());

            if (result < 1)
                return BadRequest();
            
            return Ok();
        }
    }
}