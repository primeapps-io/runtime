using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using PrimeApps.Auth.Repositories.IRepositories;

namespace PrimeApps.Auth.Repositories
{
    public class ClientRepository : BaseRepository, IClientRepository
    {
        public ClientRepository(ConfigurationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Client> Get(string clientId)
        {
            return await DbContext.Clients
            .Include(x => x.RedirectUris)
            .Include(x => x.PostLogoutRedirectUris)
            .FirstOrDefaultAsync(x => x.ClientId == clientId);
        }

        public async Task<int> Create(Client client)
        {
            DbContext.Clients.Add(client);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Client client)
        {
            return await DbContext.SaveChangesAsync();
        }
    }
}