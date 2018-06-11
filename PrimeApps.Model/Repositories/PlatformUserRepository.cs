﻿using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories
{
	public class PlatformUserRepository : RepositoryBasePlatform, IPlatformUserRepository
	{
		public PlatformUserRepository(PlatformDBContext dbContext) : base(dbContext)
		{

		}

		public async Task<PlatformUser> Get(int platformUserId)
		{
			return await DbContext.Users.Where(x => x.Id == platformUserId).SingleOrDefaultAsync();
		}

		public async Task<PlatformUser> Get(string email)
		{
			return await DbContext.Users.Where(x => x.Email == email).SingleOrDefaultAsync();
		}

		public async Task<PlatformUser> GetSettings(int platformUserId)
		{
			return await DbContext.Users
				.Include(x=> x.Setting)
				.Where(x => x.Id == platformUserId)
				.SingleOrDefaultAsync();
		}

		
		public async Task<PlatformUser> GetWithTenant(int platformUserId, int tenantId)
		{
			return await DbContext.Users.Include(x => x.TenantsAsUser.Where(z => z.TenantId == tenantId)).Where(x => x.Id == platformUserId).SingleOrDefaultAsync();
		}

		/// <summary>
		/// Gets avatar full url
		/// </summary>
		/// <returns></returns>
		public static string GetAvatarUrl(string avatar)
		{
			if (string.IsNullOrWhiteSpace(avatar))
				return string.Empty;

			var blobUrl = ConfigurationManager.AppSettings.Get("BlobUrl");

			return $"{blobUrl}/user-images/{avatar}";
		}


		public async Task UpdateAsync(PlatformUser userToEdit)
		{
			await DbContext.SaveChangesAsync();
		}

		public async Task<PlatformUser> GetUserByAutoId(int autoId)
		{
			return await DbContext.Users.Where(x => x.Id == autoId).SingleOrDefaultAsync();
		}

		public async Task<int> GetIdByEmail(string email)
		{
			return await DbContext.Users.Where(x => x.Email == email).Select(x => x.Id).SingleOrDefaultAsync();
		}

		public async Task<PlatformUser> GetWithTenants(string email)
		{
			return await DbContext.Users
				.Include(x => x.Setting)
				.Include(x => x.TenantsAsUser)
				.Include(x => x.TenantsAsOwner)
				.Where(x => x.Email == email)
				.SingleOrDefaultAsync();
		}

		public async Task<Tenant> GetTenantWithOwner(int tenantId)
		{
			return await DbContext.Tenants
				.Include(x=> x.Owner).ThenInclude(x=> x.Setting)
				.Where(x => x.Id == tenantId).SingleOrDefaultAsync();
		}

		public async Task<int> CreateUser(PlatformUser user)
		{
			DbContext.Users.Add(user);
			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> CreateTenant(Tenant tenant)
		{
			DbContext.Tenants.Add(tenant);
			return await DbContext.SaveChangesAsync();
		}

		public PlatformUser GetByEmailAndTenantId(string email, int tenantId)
		{
			var user = DbContext.UserTenants
				.Include(x => x.Tenant)
				.Include(x => x.Tenant).ThenInclude(z => z.Setting)
				.Include(x => x.Tenant).ThenInclude(z => z.License)
				.Include(x => x.Tenant).ThenInclude(z => z.App).ThenInclude(z => z.Setting)
				.Include(x => x.PlatformUser).ThenInclude(z => z.Setting)
				/*.Include(x => x.TenantsAsUser).ThenInclude(z => z.Setting)
				.Include(x => x.TenantsAsUser).ThenInclude(z => z.License)
				.Include(x => x.TenantsAsUser).ThenInclude(z => z.App).ThenInclude(z => z.Setting)*/
				.SingleOrDefault(x => x.PlatformUser.Email == email && x.TenantId == tenantId);


			return user?.PlatformUser;
		}

		public Tenant GetTenantByEmailAndAppId(string email, int appId)
		{
			var userTenant = DbContext.UserTenants
				.Include(x => x.PlatformUser)
				.Include(x => x.Tenant).ThenInclude(z => z.App)
				/*.Include(x => x.TenantsAsUser).ThenInclude(z => z.Setting)
				.Include(x => x.TenantsAsUser).ThenInclude(z => z.License)
				.Include(x => x.TenantsAsUser).ThenInclude(z => z.App).ThenInclude(z => z.Setting)*/
				.SingleOrDefault(x => x.PlatformUser.Email == email && x.Tenant.AppId == appId);


			return userTenant?.Tenant;
		}

		/// <summary>
		/// Checks if that email address is available.
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public async Task<bool> IsEmailAvailable(string email, int appId)
		{
			bool status = true;

			//get session and check the email address
			//TODO Removed
			var user = await DbContext.Users.Include(x => x.TenantsAsUser).Include(x => x.TenantsAsOwner).Where(x => x.Email == email).SingleOrDefaultAsync();
			if (user != null)
			{
				var appTenant = user.TenantsAsUser.FirstOrDefault(x => x.Tenant.AppId == appId);

				if (appTenant != null)
					status = false;
			}

			//return status.
			return status;
		}

		/// <summary>
		/// Checks if that active directory email address is available.
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public async Task<bool> IsActiveDirectoryEmailAvailable(string email)
		{
			bool status = true;

			//get session and check the email address
			//TODO Removed
			var result = false;//await DbContext.Users.Where(x => x.ActiveDirectoryEmail == email).SingleOrDefaultAsync();

			if (result != null)
			{
				//the email address exists so set the variable to false.
				status = false;
			}

			//return status.
			return status;
		}

		public async Task<List<PlatformUser>> GetAllByTenant(int tenantId)
		{
			var tenant = await DbContext.Tenants
				 .Include(x => x.TenantUsers)
				 .Include(x => x.TenantUsers.Select(z => z.PlatformUser))
				 .FirstOrDefaultAsync(x => x.Id == tenantId);

			return tenant?.TenantUsers.Select(x => x.PlatformUser).ToList();
		}

		//TODO Removed
		/*public async Task<ActiveDirectoryTenant> GetConfirmedActiveDirectoryTenant(int tenantId)
        {
            return await DbContext.ActiveDirectoryTenants.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Confirmed);
        }

        public async Task<PlatformUser> GetUserByActiveDirectoryTenantEmail(string email)
        {
            return await DbContext.Users.Where(x => x.ActiveDirectoryEmail == email).SingleOrDefaultAsync();

        }*/

		public async Task<string> GetEmail(int userId)
		{
			return await DbContext.Users.Where(x => x.Id == userId).Select(x => x.Email).SingleOrDefaultAsync();
		}

		public async Task<IList<Workgroup>> MyWorkgroups(int id)
		{
			//create result lists.
			IList<Workgroup> result = null;

			//get instances by user id, then fetch entity types with it's fields.
			result = await DbContext.Tenants
				.Where(x => x.OwnerId == id)
				.Select(i => new Workgroup //create workgroup dto and assign its fields.
				{
					TenantId = i.Id,
					Title = i.Title,
					OwnerId = i.OwnerId,
					Users = i.TenantUsers.Select(u => new UserList //get users for the instance.
					{
						Id = u.PlatformUser.Id,
						userName = u.PlatformUser.FirstName + " " + u.PlatformUser.LastName,
						email = u.PlatformUser.Email,
						hasAccount = true,
						isAdmin = u.TenantId == u.PlatformUser.Id
					}).OrderByDescending(x => x.isAdmin).ToList()
				}).ToListAsync();

			//return workgroup object.
			return result;
		}

		public int GetAppIdByDomain(string domain)
		{
			var app = DbContext.AppSettings
				.SingleOrDefault(x => x.Domain == domain);


			if (app == null)
				return 0;

			return app.AppId;
		}
	}
}
