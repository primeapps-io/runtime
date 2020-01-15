using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PrimeApps.Model.Enums;

namespace PrimeApps.Studio.Models
{
    public class ActionButtonBindingModel
    {
        [Required, StringLength(500)]
        public string ActionButtonName { get; set; }

        public ActionButtonEnum.ActionType Type { get; set; }

        public ActionButtonEnum.ActionTrigger Trigger { get; set; }

        public string Template { get; set; }

        public ActionButtonEnum.WebhhookHttpMethod MethodType { get; set; }

        public string Parameters { get; set; }

        public string Headers { get; set; }

        public string ActionButtonUrl { get; set; }

        public string CssClass { get; set; }

        public int ModuleId { get; set; }

        public List<ActionButtonPermissionBindingModel> Permissions { get; set; }

        public List<EnvironmentType> Environments { get; set; }

        public string EnvironmentValues
        {
            get
            {
                var list = new List<string>();

                foreach (var env in Environments)
                {
                    var value = (int)env;
                    list.Add(value.ToString());
                }

                return string.Join(",", list);
            }

            set
            {
                var list = value.Split(",");

                foreach (var env in list)
                {
                    Environments.Add((EnvironmentType)Enum.Parse(typeof(EnvironmentType), env));
                } 
            }
        }

    }

    public class ActionButtonPermissionBindingModel
    {
        public int? Id { get; set; }

        public int ProfileId { get; set; }

        public ActionButtonPermissionType Type { get; set; }
    }
}