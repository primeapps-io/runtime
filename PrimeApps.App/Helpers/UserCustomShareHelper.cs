using Newtonsoft.Json.Linq;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    public static class UserCustomShareHelper
    {
        public static UserCustomShare CreateEntity(UserCustomShareBindingModels userOwnerModel, IUserCustomShareRepository userOwnerRepository)
        {
            var userOwner = new UserCustomShare
            {
                UserId = userOwnerModel.UserId,
                SharedUserId = userOwnerModel.SharedUserId,
                //Users = userOwnerModel.Users,
                //UserGroups = "{"+userOwnerModel.UserGroups+"}",
                Modules = userOwnerModel.Modules
            };

            return userOwner;
        }

        public static UserCustomShare UpdateEntity(UserCustomShareBindingModels userOwnerModel, UserCustomShare userOwner, IUserCustomShareRepository userOwnerRepository)
        {
            userOwner.UserId = userOwnerModel.UserId;
            userOwner.SharedUserId = userOwnerModel.SharedUserId;
            //userOwner.Users = userOwnerModel.Users;
            //userOwner.UserGroups = "{" + userOwnerModel.UserGroups + "}";
            userOwner.Modules = userOwnerModel.Modules;

            return userOwner;
        }
    }
}