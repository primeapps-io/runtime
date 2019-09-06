using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    public interface IEnvironmentHelper
    {
        Task<List<T>> DataFilter<T>(List<T> data);
        Task<T> DataFilter<T>(T data);
        Task<string> GetEnvironmentValue();
    }

    public class EnvironmentHelper : IEnvironmentHelper
    {
        private IConfiguration _configuration;

        public EnvironmentHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<T>> DataFilter<T>(List<T> data)
        {
            if (data == null || data.Count < 1)
                return data;

            var environmentType = await GetEnvironmentValue();

            var prop = typeof(T).GetProperty("Environment");
            var newData = new List<T>();

            foreach (var item in data)
            {
                if (prop.GetValue(item) != null)
                {
                    var value = prop.GetValue(item).ToString();

                    if (value != null)
                    {
                        if (value.Contains(environmentType))
                            newData.Add(item);
                    }
                }
            }

            return newData;
        }

        public async Task<T> DataFilter<T>(T data)
        {
            if (data == null)
                return default(T);

            var environmentType = await GetEnvironmentValue();

            var prop = typeof(T).GetProperty("Environment");

            if (prop.GetValue(data) == null)
                return default(T);

            var value = prop.GetValue(data).ToString();

            if (value != null && value.Contains(environmentType))
                return data;
            else
                return default(T);

        }

        public async Task<string> GetEnvironmentValue()
        {

            var environment = !string.IsNullOrEmpty(_configuration.GetValue("AppSettings:Environment", string.Empty)) ? _configuration.GetValue("AppSettings:Environment", string.Empty) : "development";
            //var environmentType = Enum.Parse<EnvironmentType>( environment.ToString(),false );
            //var e = Enum.TryParse("development", out EnvironmentType myStatus);
            //var a = (EnvironmentType)Enum.Parse(typeof(EnvironmentType), environment);

            string value = null;

            switch (environment)
            {
                case "development":
                    value = "1";
                    break;
                case "test":
                    value = "2";
                    break;
                case "product":
                    value = "3";
                    break;
            }

            return value;
        }
    }
}
