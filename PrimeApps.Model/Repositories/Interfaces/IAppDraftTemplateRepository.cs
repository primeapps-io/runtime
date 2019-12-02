using Newtonsoft.Json.Linq;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Common;
using System.Linq;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IAppDraftTemplateRepository : IRepositoryBaseStudio
    {
        Task<List<AppDraftTemplate>> GetAll(int appId);
        Task<int> Create(AppDraftTemplate template);
        Task<int> Update(AppDraftTemplate template);
        IQueryable<AppDraftTemplate> Find();
        int Count(int appId);
        Task<AppDraftTemplate> Get(int id);
    }
}