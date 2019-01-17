using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IProfileRepository : IRepositoryBaseTenant
    {
        Task AddModuleAsync(int moduleId);
        Task<Profile> GetProfileById(int id);
        Task<List<Profile>> GetByParentId(int parentId);
        Task AddUserAsync(int userId, int profileId);
        Task CreateAsync(ProfileDTO newProfileDTO, string tenantLanguage);
        Task<IEnumerable<Profile>> GetAll();
        Task<IEnumerable<ProfileWithUsersDTO>> GetAllProfiles();
        Task<IEnumerable<ProfileLightDTO>> GetUserProfilesForCache();
        Task RemoveAsync(int profileId, int replacementProfileId);
        Task RemoveModule(int moduleId);
        Task UpdateAsync(ProfileDTO updatedProfileDTO, string tenantLanguage);
        Task<Profile> GetDefaultAdministratorProfileAsync();
        Task<Profile> GetDefaultUserProfile();
        Task<Profile> GetByIdBasic(int id);
        Task<int> DeleteSoft(Profile profile);


    }
}