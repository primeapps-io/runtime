using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Auth.Models.AccountViewModels
{
	public class ExternalProviderModel
	{
		public string DisplayName { get; set; }
		public string AuthenticationScheme { get; set; }
	}
}
