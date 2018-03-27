using System;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using PrimeApps.App.Helpers;

namespace PrimeApps.App.ActionFilters
{
    public class SnakeCaseAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Clear();
            var serializerSettings = JsonHelper.GetDefaultJsonSerializerSettings();

            var formatter = new JsonMediaTypeFormatter
            {
                SerializerSettings = serializerSettings
            };

            controllerSettings.Formatters.Add(formatter);
            controllerSettings.Services.Replace(typeof(IHttpActionSelector), new SnakeCaseActionSelector());
        }
    }
}