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
        List<T> DataFilter<T>(List<T> data);
        T DataFilter<T>(T data);
        string GetEnvironmentValue();
    }

    public class EnvironmentHelper : IEnvironmentHelper
    {
        private IConfiguration _configuration;

        public EnvironmentHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<T> DataFilter<T>(List<T> data)
        {
            if (data == null || data.Count < 1)
                return data;

            var environmentType = GetEnvironmentValue();

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

        public T DataFilter<T>(T data)
        {
            if (data == null)
                return default(T);

            var environmentType = GetEnvironmentValue();

            var prop = typeof(T).GetProperty("Environment");

            if (prop.GetValue(data) == null)
                return default(T);

            var value = prop.GetValue(data).ToString();

            if (value != null && value.Contains(environmentType))
                return data;
            else
                return default(T);
        }

        public string GetEnvironmentValue()
        { 
            var environment = !string.IsNullOrEmpty(_configuration.GetValue("AppSettings:Environment", string.Empty)) ? _configuration.GetValue("AppSettings:Environment", string.Empty) : "development";
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
