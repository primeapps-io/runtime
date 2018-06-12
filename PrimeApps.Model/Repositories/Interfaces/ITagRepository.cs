using Newtonsoft.Json.Linq;
using OfisimCRM.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using OfisimCRM.DTO.Cache;

namespace OfisimCRM.Model.Repositories.Interfaces
{
    public interface ITagRepository
    {
        Task<ICollection<Tag>> GetAllBasic();
        Task<ICollection<Tag>> GetByFieldId(int id);
        Task<Tag> GetById(int id);
        Task<int> Create(Tag tag);
    }
}
