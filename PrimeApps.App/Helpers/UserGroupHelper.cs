using System.Collections.Generic;
using System.Linq;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Application;

namespace PrimeApps.App.Helpers
{
    public static class UserGroupHelper
    {
        public static UserGroup CreateEntity(UserGroupBindingModel userGroupModel, ICollection<TenantUserGroup> users)
        {
            var userGroup = new UserGroup
            {
                Name = userGroupModel.Name,
                Description = userGroupModel.Description,
                Users = users
            };

            return userGroup;
        }

        public static void UpdateEntity(UserGroupBindingModel userGroupModel, UserGroup userGroup, ICollection<TenantUserGroup> users)
        {
            userGroup.Name = userGroupModel.Name;
            userGroup.Description = userGroupModel.Description;
            userGroup.Users = users;
        }
    }
}