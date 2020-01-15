using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;

namespace PrimeApps.Auth.Repositories.IRepositories
{
    public interface IClientRepository
    {
        Task<Client> Get(string clientId);
        Task<int> Create(Client client);
        Task<int> Update(Client client);
    }
}