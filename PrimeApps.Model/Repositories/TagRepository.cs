using System.Collections.Generic;
using OfisimCRM.Model.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using Microsoft.EntityFrameworkCore;

namespace OfisimCRM.Model.Repositories
{
    public class TagRepository : RepositoryBaseTenant, ITagRepository
    {
        public TagRepository(TenantDBContext dbContext) : base(dbContext) { }



        public async Task<ICollection<Tag>> GetAllBasic()
        {
            var tags = DbContext.Tags.Where(x => !x.Deleted);

            return await tags.ToListAsync();
        }

        public async Task<ICollection<Tag>> GetByFieldId(int id)
        {
            var tags = DbContext.Tags.Where(x => !x.Deleted && x.FieldId == id);

            return await tags.ToListAsync();
        }


        public async Task<Tag> GetById(int id)
        {
            var tag = DbContext.Tags.Where(x => !x.Deleted && x.Id == id);

            return tag.FirstOrDefault();
        }

        public async Task<int> Create(Tag tag)
        {
            DbContext.Tags.Add(tag);

            return await DbContext.SaveChangesAsync();
        }


        public async Task<int> DeleteSoft(Tag tag)
        {
            tag.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(Tag tag)
        {
            DbContext.Tags.Remove(tag);

            return await DbContext.SaveChangesAsync();
        }

    }
}
