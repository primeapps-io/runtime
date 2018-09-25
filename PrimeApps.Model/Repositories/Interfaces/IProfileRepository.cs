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
        Task AddUserAsync(int userId, int profileId);
        Task CreateAsync(ProfileDTO newProfileDTO);
        Task<IEnumerable<Profile>> GetAll();
        Task<IEnumerable<ProfileWithUsersDTO>> GetAllProfiles();
        Task<IEnumerable<ProfileLightDTO>> GetUserProfilesForCache();
        Task RemoveAsync(int profileId, int replacementProfileId);
        Task RemoveModule(int moduleId);
        Task UpdateAsync(ProfileDTO updatedProfileDTO);
        Task<Profile> GetDefaultAdministratorProfileAsync();
        Task<Profile> GetDefaultUserProfile();

    }
}