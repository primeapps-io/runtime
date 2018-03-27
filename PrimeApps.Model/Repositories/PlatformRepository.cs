using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
using PrimeApps.Model.Entities.Platform;
using Microsoft.EntityFrameworkCore;

namespace PrimeApps.Model.Repositories
{
    public class PlatformRepository : RepositoryBasePlatform, IPlatformRepository
    {
        public PlatformRepository(PlatformDBContext dbContext) : base(dbContext) { }

        public async Task<App> AppGetById(int id, int userId)
        {
            var note = await DbContext.Apps
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id && x.UserId == userId);

            return note;
        }

        public async Task<List<App>> AppGetAll(int userId)
        {
            var note = await DbContext.Apps
                .Where(x => !x.Deleted && x.UserId == userId)
                .ToListAsync();

            return note;
        }

        public async Task<int> AppCreate(App app)
        {
            app.UserId = CurrentUser.UserId;
            DbContext.Apps.Add(app);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> AppUpdate(App app)
        {
            app.UserId = CurrentUser.UserId;
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> AppDeleteSoft(App app)
        {
            app.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> AppDeleteHard(App app)
        {
            DbContext.Apps.Remove(app);

            return await DbContext.SaveChangesAsync();
        }
    }
}
