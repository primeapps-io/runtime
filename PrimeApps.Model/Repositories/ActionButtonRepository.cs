using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Common.ActionButton;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class ActionButtonRepository : RepositoryBaseTenant, IActionButtonRepository
    {
        public ActionButtonRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext,
            configuration)
        {
        }

        public async Task<ICollection<ActionButtonViewModel>> GetByModuleId(int id)
        {
            var actionButtons = new List<ActionButtonViewModel>();

            var actionButtonList = await DbContext.ActionButtons
                .Include(x => x.Permissions)
                .Where(r => r.ModuleId == id && r.Deleted == false)
                .ToListAsync();

            foreach (var actionButtonItem in actionButtonList)
            {
                var actionButton = new ActionButtonViewModel
                {
                    Id = actionButtonItem.Id,
                    ActionType = actionButtonItem.Type,
                    Name = actionButtonItem.Name,
                    Template = actionButtonItem.Template,
                    ModuleId = id,
                    Icon = actionButtonItem.Icon,
                    CssClass = actionButtonItem.CssClass,
                    Url = actionButtonItem.Url,
                    Trigger = actionButtonItem.Trigger,
                    DependentField = actionButtonItem.DependentField,
                    Dependent = actionButtonItem.Dependent,
                    MethodType = actionButtonItem.MethodType,
                    Parameters = actionButtonItem.Parameters,
                    Headers = actionButtonItem.Headers,
                    Environment = actionButtonItem.Environment
                };

                if (actionButtonItem.Permissions != null && actionButtonItem.Permissions.Count > 0)
                {
                    actionButton.Permissions = new List<ActionButtonPermissionViewModel>();

                    foreach (var permission in actionButtonItem.Permissions)
                    {
                        actionButton.Permissions.Add(new ActionButtonPermissionViewModel
                        {
                            Id = permission.Id,
                            ProfileId = permission.ProfileId,
                            Type = permission.Type
                        });
                    }
                }

                actionButtons.Add(actionButton);
            }

            return actionButtons;
        }

        public async Task<ActionButton> GetById(int id)
        {
            var actionbutton = await DbContext.ActionButtons
                .Include(x => x.CreatedBy)
                .Include(x => x.Permissions)
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return actionbutton;
        }

        public async Task<ActionButton> GetByIdBasic(int id)
        {
            var actionbutton = await DbContext.ActionButtons
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return actionbutton;
        }

        public async Task<int> Create(ActionButton actionbutton)
        {
            DbContext.ActionButtons.Add(actionbutton);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(ActionButton actionbutton)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(ActionButton actionbutton)
        {
            actionbutton.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Count(int id)
        {
            var count = DbContext.ActionButtons
                .Where(x => !x.Deleted);

            if (id > 0)
                count = count.Where(x => x.ModuleId == id);

            return await count.CountAsync();
        }

        public async Task<ICollection<ActionButton>> Find(int id, PaginationModel paginationModel)
        {
            var actionButtons = DbContext.ActionButtons
                .Where(x => !x.Deleted)
                .Include(x => x.Permissions)
                .OrderByDescending(x => x.Id)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit);

            if (id > 0)
                actionButtons = actionButtons.Where(x => x.ModuleId == id);

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(ActionButton).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

                if (paginationModel.OrderType == "asc")
                {
                    actionButtons = actionButtons.OrderBy(x => propertyInfo.GetValue(x, null));
                }
                else
                {
                    actionButtons = actionButtons.OrderByDescending(x => propertyInfo.GetValue(x, null));
                }
            }

            return await actionButtons.ToListAsync();
        }
    }
}