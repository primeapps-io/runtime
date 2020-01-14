﻿using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Common.Messaging;

namespace PrimeApps.Model.Repositories
{
	public class NotificationRepository : RepositoryBaseTenant, INotificationRepository
	{
		public NotificationRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

		public async Task<Notification> GetById(int id)
		{
			var notification = await DbContext.Notifications
				.Include(x => x.CreatedBy)
				.FirstOrDefaultAsync(r => r.Id == id && !r.Deleted);

			return notification;
		}

		public async Task<List<Setting>> GetSetting(MessageDTO queueItem)
		{
			
			var settings = DbContext.Settings
				.Include(x => x.CreatedBy)
				.Where(r => r.Type == SettingType.Email && !r.Deleted);

			if (queueItem.AccessLevel == AccessLevelEnum.Personal)
				settings = settings.Where(r => r.UserId == CurrentUser.UserId);
			else
				settings = settings.Where(r => !r.UserId.HasValue);

			return await settings.ToListAsync();
		}
	}
}

