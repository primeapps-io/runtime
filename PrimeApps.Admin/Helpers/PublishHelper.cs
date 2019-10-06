using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;

namespace PrimeApps.Admin.Helpers
{
    public interface IPublishHelper
    {
        Task<bool> IsActiveUpdateButton(int appId);
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

        public async Task<bool> IsActiveUpdateButton(int appId)
        {
            var tenantIds = await _tenantRepository.GetByAppId(appId);
            var firstTime = await _releaseRepository.FirstTime(appId);

            if (firstTime)
                return false;

            return tenantIds != null && tenantIds.Count >= 1;
        }
    }
}