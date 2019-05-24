using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
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
    [Route("api/[controller]")]
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

            var newClient = new IdentityServer4.Models.Client
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                AllowedGrantTypes = client.AllowedGrantTypes.Split(";"),
                AllowRememberConsent = false,
                AlwaysSendClientClaims = true,
                RequireConsent = false,

                ClientSecrets =
                {
                    new IdentityServer4.Models.Secret(client.ClientSecrets.Sha256())
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
        public async Task<IActionResult> Update([FromRoute]string clientId, [FromBody]UpdateClientBindingModel clientModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = await _clientRepository.Get(clientId);

            if (client == null)
                return NotFound();

            var newClient = new IdentityServer4.Models.Client
            {
                ClientName = clientModel.ClientName ?? client.ClientName,
                AllowRememberConsent = clientModel.AllowRememberConsent ?? client.AllowRememberConsent,
                AlwaysSendClientClaims = clientModel.AlwaysSendClientClaims ?? client.AlwaysSendClientClaims,
                RequireConsent = clientModel.RequireConsent ?? client.RequireConsent,
                AccessTokenLifetime = clientModel.AccessTokenLifetime ?? client.AccessTokenLifetime
            };
            if (!string.IsNullOrEmpty(clientModel.AllowedGrantTypes))
                newClient.AllowedGrantTypes = clientModel.AllowedGrantTypes.Split(";");

            if (!string.IsNullOrEmpty(clientModel.RedirectUris))
                newClient.RedirectUris = clientModel.RedirectUris.Split(";");

            if (!string.IsNullOrEmpty(clientModel.PostLogoutRedirectUris))
                newClient.PostLogoutRedirectUris = clientModel.PostLogoutRedirectUris.Split(";");

            if (!string.IsNullOrEmpty(clientModel.AllowedScopes))
                newClient.AllowedScopes = clientModel.AllowedScopes.Split(";");

            var result = await _clientRepository.Update(newClient.ToEntity());

            if (result < 1)
                return BadRequest();

            return Ok();
        }

        [Route("add_url"), HttpPost]
        public async Task<IActionResult> AddUrl([FromBody]AddClientUrlBindingModel clientModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(clientModel.ClientId))
                clientModel.ClientId = "primeapps_app";

            var client = await _clientRepository.Get(clientModel.ClientId);

            if (client == null)
                return NotFound();

            foreach (var url in clientModel.Urls.Split(";"))
            {
                client.RedirectUris.Add(new ClientRedirectUri()
                {
                    Client = client,
                    RedirectUri = url + "/signin-oidc"
                });

                client.PostLogoutRedirectUris.Add(new ClientPostLogoutRedirectUri()
                {
                    Client = client,
                    PostLogoutRedirectUri = url + "/signout-callback-oidc"
                });
            }

            var result = await _clientRepository.Update(client);

            if (result < 1)
                return BadRequest();

            return Ok();
        }
    }
}