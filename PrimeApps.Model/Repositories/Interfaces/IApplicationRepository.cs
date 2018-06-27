using PrimeApps.Model.Entities.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IApplicationRepository : IRepositoryBasePlatform
	{
		App Get(string domain);
		App GetWithAuth(string domain);
		App Get(int id);
		TeamApp Get(string organizationCode, string appCode);
	}
}
