using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Services
{
	public class HangfireActivator : Hangfire.JobActivator
	{
		private readonly IServiceProvider _serviceProvider;

		public HangfireActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public override object ActivateJob(Type type)
		{
			return _serviceProvider.GetService(type);
		}
	}
}
