﻿using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IDeploymentComponentRepository : IRepositoryBaseTenant
    {
        Task<int> Count(int functionId);
        Task<DeploymentComponent> Get(int id);
        Task<int> CurrentBuildNumber();
        Task<ICollection<DeploymentComponent>> Find(int functionId, PaginationModel paginationModel);
        Task<int> Create(DeploymentComponent deployment);
        Task<int> Update(DeploymentComponent deployment);
        Task<int> Delete(DeploymentComponent deployment);
    }
}
