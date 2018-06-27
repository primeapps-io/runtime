using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrimeApps.Model.Repositories.Interfaces
{
	public class ApplicationRepository : RepositoryBasePlatform, IApplicationRepository
	{
		public ApplicationRepository(PlatformDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }
		public App Get(string domain)
		{
			var app = DbContext.Apps
				.Include(x => x.Setting)
				.FirstOrDefault(x => x.Setting.Domain == domain);

			return app;
		}
		public App Get(int id)
		{
			var app = DbContext.Apps
				.Include(x => x.Setting)
				.FirstOrDefault(x => x.Id == id);

			return app;
		}

		public TeamApp Get(string organizationName, string appName)
		{
			var app = DbContext.TeamApps
				.Include(y => y.App).ThenInclude(x => x.Setting)
				.Include(y => y.Team).ThenInclude(x => x.Organization)
				.FirstOrDefault(x => x.Team.Organization.Name == organizationName && x.App.Name == appName);

			return app == null ? null : app;
		}
		public App GetWithAuth(string domain)
		{
			var app = DbContext.Apps
				.Include(x => x.Setting)
				.FirstOrDefault(x => x.Setting.AuthDomain == domain);

			return app;
		}

		public int GetAppIdWithDomain(string domain)
		{
			var app = DbContext.AppSettings
				.FirstOrDefault(x => x.Domain == domain);

			if (app == null)
				return 0;

			return app.AppId;
		}

	}
}
