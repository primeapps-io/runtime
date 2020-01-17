using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Helpers;
using Hangfire;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class ProfileRepository : RepositoryBaseTenant, IProfileRepository
    {
        private Warehouse _warehouse;

        public ProfileRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext,
            configuration)
        {
        }

        public ProfileRepository(TenantDBContext dbContext, IConfiguration configuration, Warehouse warehouse) : base(
            dbContext, configuration)
        {
            _warehouse = warehouse;
        }

        public async Task<Profile> CreateAsync(ProfileDTO newProfileDTO, string tenantLanguage)
        {
            Profile newProfile = new Profile()
            {
                DescriptionEn = newProfileDTO.DescriptionEn,
                DescriptionTr = newProfileDTO.DescriptionTr,
                HasAdminRights = newProfileDTO.HasAdminRights,
                IsPersistent = false,
                SendEmail = newProfileDTO.SendEmail,
                SendSMS = newProfileDTO.SendSMS,
                WordPdfDownload = newProfileDTO.WordPdfDownload,
                LeadConvert = newProfileDTO.LeadConvert,
                DocumentSearch = newProfileDTO.DocumentSearch,
                ImportData = newProfileDTO.ImportData,
                ExportData = newProfileDTO.ExportData,
                Tasks = newProfileDTO.Tasks,
                Calendar = newProfileDTO.Calendar,
                Newsfeed = newProfileDTO.Newsfeed,
                NameEn = newProfileDTO.NameEn,
                NameTr = string.IsNullOrEmpty(newProfileDTO.NameTr) ? newProfileDTO.NameEn : newProfileDTO.NameTr,
                Dashboard = newProfileDTO.Dashboard,
				CloseSmtpSettings = newProfileDTO.CloseSmtpSettings,
                Home = newProfileDTO.Home,
                CollectiveAnnualLeave = newProfileDTO.CollectiveAnnualLeave,
                StartPage = newProfileDTO.StartPage,
                ParentId = newProfileDTO.ParentId
            };

            DbContext.ProfilePermissions.Add(new ProfilePermission()
            {
                Type = EntityType.Document,
                Profile = newProfile,
                Modify = true,
                Read = true,
                Write = true,
                Remove = true
            });

            //Front tarafından default olarak geliyor. Yoksa Rapor iznini çokluyordu.
            //DbContext.ProfilePermissions.Add(new ProfilePermission()
            //{
            //    Type = EntityType.Report,
            //    Profile = newProfile,
            //    Modify = true,
            //    Read = true,
            //    Write = true,
            //    Remove = true
            //});

            DbContext.ProfilePermissions.Add(new ProfilePermission()
            {
                Type = EntityType.Newsfeed,
                Profile = newProfile,
                Modify = true,
                Read = true,
                Write = true,
                Remove = true
            });

            foreach (ProfilePermissionDTO permission in newProfileDTO.Permissions)
            {
                DbContext.ProfilePermissions.Add(new ProfilePermission()
                {
                    ModuleId = permission.ModuleId,
                    Modify = permission.Modify,
                    Read = permission.Read,
                    Remove = permission.Remove,
                    Write = permission.Write,
                    Profile = newProfile,
                    Type = (EntityType)permission.Type,
                });
            }

            DbContext.Profiles.Add(newProfile);
            var result = await DbContext.SaveChangesAsync();
            if (result > 0)
            {
                return newProfile;
            }
            else
            {
                throw new Exception("");
            }


            //if (result > 0)
            //{
            //    //if (string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
            //    //    throw new Exception("Warehouse cannot be null during create/update/delete record.");

            //    if (_warehouse?.DatabaseName != "0")
            //    {
            //        BackgroundJob.Enqueue(() => _warehouse.CreateProfile(newProfile.Id, _warehouse.DatabaseName, CurrentUser.TenantId, tenantLanguage));
            //    }
            //}
        }

        public async Task UpdateAsync(ProfileDTO updatedProfileDTO, string tenantLanguage)
        {
            Profile profileToUpdate = await DbContext.Profiles.Include(x => x.Permissions)
                .Where(x => x.Id == updatedProfileDTO.Id).SingleOrDefaultAsync();

            if (profileToUpdate == null) return;

            profileToUpdate.NameEn = updatedProfileDTO.NameEn;
            profileToUpdate.NameTr = updatedProfileDTO.NameTr;
            profileToUpdate.DescriptionEn = updatedProfileDTO.DescriptionEn;
            profileToUpdate.DescriptionTr = updatedProfileDTO.DescriptionTr;
            profileToUpdate.HasAdminRights = updatedProfileDTO.HasAdminRights;
            profileToUpdate.SendSMS = updatedProfileDTO.SendSMS;
            profileToUpdate.SendEmail = updatedProfileDTO.SendEmail;
            profileToUpdate.ExportData = updatedProfileDTO.ExportData;
            profileToUpdate.ImportData = updatedProfileDTO.ImportData;
            profileToUpdate.WordPdfDownload = updatedProfileDTO.WordPdfDownload;
            profileToUpdate.DocumentSearch = updatedProfileDTO.DocumentSearch;
            profileToUpdate.LeadConvert = updatedProfileDTO.LeadConvert;
            profileToUpdate.Tasks = updatedProfileDTO.Tasks;
            profileToUpdate.Calendar = updatedProfileDTO.Calendar;
            profileToUpdate.Newsfeed = updatedProfileDTO.Newsfeed;
            profileToUpdate.Report = updatedProfileDTO.Report;
            profileToUpdate.Newsfeed = updatedProfileDTO.Newsfeed;
            profileToUpdate.Dashboard = updatedProfileDTO.Dashboard;
            profileToUpdate.CloseSmtpSettings = updatedProfileDTO.CloseSmtpSettings;
            profileToUpdate.Home = updatedProfileDTO.Home;
            profileToUpdate.CollectiveAnnualLeave = updatedProfileDTO.CollectiveAnnualLeave;
            profileToUpdate.StartPage = updatedProfileDTO.StartPage;
            profileToUpdate.ParentId = updatedProfileDTO.ParentId;

            DbContext.ProfilePermissions.RemoveRange(profileToUpdate.Permissions);

            foreach (ProfilePermissionDTO newPermission in updatedProfileDTO.Permissions)
            {
                var profilePermission = new ProfilePermission()
                {
                    Modify = newPermission.Modify,
                    Read = newPermission.Read,
                    Remove = newPermission.Remove,
                    Write = newPermission.Write,
                    Profile = profileToUpdate,
                };

                if (newPermission.ModuleId.HasValue)
                {
                    profilePermission.Type = EntityType.Module;
                    profilePermission.ModuleId = newPermission.ModuleId;
                }
                else
                {
                    if (newPermission.Type == 1)
                    {
                        profilePermission.Type = EntityType.Document;
                    }

                    if (newPermission.Type == 2)
                    {
                        profilePermission.Type = EntityType.Report;
                    }

                    if (newPermission.Type == 3)
                    {
                        profilePermission.Type = EntityType.Newsfeed;
                    }
                }


                DbContext.ProfilePermissions.Add(profilePermission);
            }

            var result = await DbContext.SaveChangesAsync();

            //if (result > 0)
            //{
            //    //if (string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
            //    //    throw new Exception("Warehouse cannot be null during create/update/delete record.");

            //    if (_warehouse?.DatabaseName != "0")
            //    {
            //        BackgroundJob.Enqueue(() => _warehouse.UpdateProfile(profileToUpdate.Id, _warehouse.DatabaseName, CurrentUser.TenantId, tenantLanguage));
            //    }
            //}
        }

        /// <summary>
        /// Removes a profile by switching it with a replacement.
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="replacementProfileId"></param>
        /// <param name="instanceID"></param>
        /// <param name="session"></param>
        public async Task RemoveAsync(int profileId, int replacementProfileId)
        {
            /// get profile to be removed and the replacement profile
            Profile profileToDelete = await DbContext.Profiles.Include(x => x.Users).Where(x => x.Id == profileId)
                .SingleOrDefaultAsync();
            /// get the list of affected profile to user relations and invited users who has a relation with the removed profile.
            /// TODO: Pending Share Request
            //IList<crmPendingShareRequests> affectedInvitations = session.Query<crmPendingShareRequests>().Where(x => x.Profile == profileToDelete).ToList();

            /// create batch update for the new relations.
            foreach (TenantUser user in profileToDelete.Users)
            {
                user.ProfileId = replacementProfileId;
            }

            /// create batch update for the invitations.
            /// TODO: Pending Share Request
            //foreach (crmPendingShareRequests invitation in affectedInvitations)
            //{
            //    invitation.Profile = replacementProfile;
            //    session.Update(invitation);
            //}

            DbContext.Profiles.Remove(profileToDelete);

            await DbContext.SaveChangesAsync();
        }

        public async Task AddUserAsync(int userId, int profileId)
        {
            var user = await DbContext.Users.FindAsync(userId);
            user.ProfileId = profileId;

            await DbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProfileLightDTO>> GetUserProfilesForCache()
        {
            return await DbContext.Profiles
                .Include(x => x.Users)
                .Include(x => x.Permissions)
                .Select(x => new ProfileLightDTO()
                {
                    HasAdminRights = x.HasAdminRights,
                    SendSMS = x.SendSMS,
                    SendEmail = x.SendEmail,
                    ImportData = x.ImportData,
                    ExportData = x.ExportData,
                    WordPdfDownload = x.WordPdfDownload,
                    LeadConvert = x.LeadConvert,
                    DocumentSearch = x.DocumentSearch,
                    Tasks = x.Tasks,
                    Calendar = x.Calendar,
                    Newsfeed = x.Newsfeed,
                    Report = x.Report,
                    Dashboard = x.Dashboard,
                    Home = x.Home,
                    CollectiveAnnualLeave = x.CollectiveAnnualLeave,
                    StartPage = x.StartPage,
                    ParentId = x.ParentId,
                    CreatedBy = x.CreatedById,
                    IsPersistent = x.IsPersistent,
                    UserIDs = x.Users.Select(z => z.Id).ToList(),
                    Permissions = x.Permissions.Select(y => new ProfilePermissionLightDTO()
                    {
                        Type = (int)y.Type,
                        Modify = y.Modify,
                        Read = y.Read,
                        Remove = y.Remove,
                        Write = y.Write,
                        ModuleId = y.ModuleId
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<Profile> GetProfileById(int id)
        {
            var profile = await DbContext.Profiles
                .Include(x => x.Permissions)
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return profile;
        }

        public async Task<IEnumerable<ProfileWithUsersDTO>> GetAllProfiles(string language = "en")
        {
            return await DbContext.Profiles
                .Select(x => new ProfileWithUsersDTO()
                {
                    ID = x.Id,
                    Description = language == "en" ? x.DescriptionEn : x.DescriptionTr,
                    Name = language == "en" ? x.NameEn : x.NameTr,
                    IsPersistent = x.IsPersistent,
                    HasAdminRights = x.HasAdminRights,
                    SendEmail = x.SendEmail,
                    SendSMS = x.SendSMS,
                    ExportData = x.ExportData,
                    ImportData = x.ImportData,
                    WordPdfDownload = x.WordPdfDownload,
                    LeadConvert = x.LeadConvert,
                    CreatedBy = x.CreatedById,
                    DocumentSearch = x.DocumentSearch,
                    Tasks = x.Tasks,
                    Calendar = x.Calendar,
                    Newsfeed = x.Newsfeed,
                    Report = x.Report,
                    Dashboard = x.Dashboard,
                    Home = x.Home,
                    SmtpSettings = x.CloseSmtpSettings,
                    CollectiveAnnualLeave = x.CollectiveAnnualLeave,
                    StartPage = x.StartPage,
                    ParentId = x.ParentId,
                    UserIds = x.Users.Select(z => z.Id).ToList(),
                    Permissions = x.Permissions.Select(y => new ProfilePermissionDTO()
                    {
                        ID = y.Id,
                        ModuleId = y.ModuleId,
                        Type = (int)y.Type,
                        Modify = y.Modify,
                        Read = y.Read,
                        Remove = y.Remove,
                        Write = y.Write
                    }).ToList()
                }).ToListAsync();
        }

        /// <summary>
        /// Gets all profiles belongs to an instance.
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Profile>> GetAll()
        {
            return await DbContext.Profiles
                .Where(x => !x.Deleted) 
                .OrderBy(x => x.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Adds an entity to all permission profiles of an instance. By default the new added entity is only available to the administrator profile.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="instanceID"></param>
        /// <param name="session"></param>
        public async Task AddModuleAsync(int moduleId)
        {
            IEnumerable<Profile> profilesOfInstance = await GetAll();

            foreach (Profile profile in profilesOfInstance)
            {
                ProfilePermission perm = new ProfilePermission()
                {
                    ModuleId = moduleId,
                    Type = EntityType.Module,
                    Modify = profile.IsPersistent,
                    Profile = profile,
                    Read = profile.IsPersistent,
                    Remove = profile.IsPersistent,
                    Write = profile.IsPersistent
                };

                DbContext.ProfilePermissions.Add(perm);
            }

            await DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a deleted entity type from all related profiles.
        /// Warning: Since there is no instance id attached to this method be careful when you are using it!
        /// </summary>
        /// <param name="entityTypeID"></param>
        /// <param name="session"></param>
        public async Task RemoveModule(int moduleId)
        {
            DbContext.ProfilePermissions.RemoveRange(DbContext.ProfilePermissions.Where(x => x.Module.Id == moduleId));
            await DbContext.SaveChangesAsync();
        }

        public async Task<Profile> GetDefaultAdministratorProfileAsync()
        {
            return await DbContext.Profiles.Where(x => x.IsPersistent && x.HasAdminRights).SingleOrDefaultAsync();
        }

        public async Task<Profile> GetDefaultUserProfile()
        {
            return await DbContext.Profiles.Where(x => x.IsPersistent && !x.HasAdminRights).SingleOrDefaultAsync();
        }

        public async Task<List<Profile>> GetByParentId(int parentId)
        {
            return await DbContext.Profiles.Where(x => x.ParentId == parentId).ToListAsync();
        }

        public async Task<Profile> GetByIdBasic(int id)
        {
            var profile = await DbContext.Profiles
                .Where(x => !x.Deleted && x.Id == id)
                .FirstOrDefaultAsync();

            return profile;
        }

        public async Task<int> DeleteSoft(Profile profile)
        {
            profile.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public int Count()
        {
            var count = DbContext.Profiles
                .Where(x => !x.Deleted).Count();
            return count;
        }

        public IQueryable<Profile> Find()
        {
            var profiles = DbContext.Profiles
                .Where(x => !x.Deleted)
                .Include(x => x.Permissions)
                .OrderByDescending(x => x.Id);

            return profiles;
        }

        private async Task<List<ProfileWithUsersDTO>> GetPaginationQuery(PaginationModel paginationModel)
        {
            return await DbContext.Profiles
                .Where(x => !x.Deleted)
                .Select(x => new ProfileWithUsersDTO()
                {
                    ID = x.Id,
                    Description = x.DescriptionEn,
                    Name = x.NameEn,
                    IsPersistent = x.IsPersistent,
                    HasAdminRights = x.HasAdminRights,
                    SendEmail = x.SendEmail,
                    SendSMS = x.SendSMS,
                    ExportData = x.ExportData,
                    ImportData = x.ImportData,
                    WordPdfDownload = x.WordPdfDownload,
                    LeadConvert = x.LeadConvert,
                    CreatedBy = x.CreatedById,
                    DocumentSearch = x.DocumentSearch,
                    Tasks = x.Tasks,
                    Calendar = x.Calendar,
                    Newsfeed = x.Newsfeed,
                    Report = x.Report,
                    Dashboard = x.Dashboard,
                    Home = x.Home,
                    SmtpSettings = x.CloseSmtpSettings,
                    CollectiveAnnualLeave = x.CollectiveAnnualLeave,
                    StartPage = x.StartPage,
                    ParentId = x.ParentId,
                    UserIds = x.Users.Select(z => z.Id).ToList(),
                    Permissions = x.Permissions.Select(y => new ProfilePermissionDTO()
                    {
                        ID = x.Id,
                        ModuleId = y.ModuleId,
                        Type = (int)y.Type,
                        Modify = y.Modify,
                        Read = y.Read,
                        Remove = y.Remove,
                        Write = y.Write
                    }).ToList()
                }).ToListAsync();
        }
    }
}