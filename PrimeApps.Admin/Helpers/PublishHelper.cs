using System.Collections.Generic;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;

namespace PrimeApps.Admin.Helpers
{
    public interface IPublishHelper
    {
        Task<IList<int>> GetTenantIds(int appId);
    }

    public class PublishHelper : IPublishHelper
    {
        private readonly IReleaseRepository _releaseRepository;
        private readonly ITenantRepository _tenantRepository;

        public PublishHelper(IReleaseRepository releaseRepository, ITenantRepository tenantRepository)
        {
            _releaseRepository = releaseRepository;
            _tenantRepository = tenantRepository;
        }

        public async Task<IList<int>> GetTenantIds(int appId)
        {
            var tenantIds = await _tenantRepository.GetIdsByAppId(appId);
            var firstTime = await _releaseRepository.FirstTime(appId);

            return firstTime ? null : tenantIds;
        }
    }
}