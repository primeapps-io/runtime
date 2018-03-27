using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    /// <summary>
    /// Loads the localization resources
    /// </summary>
    public class Localization
    {
        CultureInfo Culture { get; set; }
        private ResourceManager Resource { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Localization"/> class.
        /// </summary>
        /// <param name="culture">Culture settings</param>
        /// <param name="name">Namespace of the resource</param>
        public Localization(string culture, Type resource)
        {
            Culture = new CultureInfo(culture);
            Resource = new ResourceManager(resource.FullName, this.GetType().Assembly);
        }
        /// <summary>
        /// Gets the string value of a localization resource.
        /// </summary>
        /// <param name="name">Name of the resource</param>
        /// <returns>System.String.</returns>
        public string GetString(string name)
        {

            string value = "";

            try
            {
                value = Resource.GetString(name, Culture);

            }
            catch (Exception)
            {
                value = name;
                throw new  CultureNotFoundException(String.Format("Resource not found for Culture:{0}", Culture.EnglishName));
            }

            if (value == null)
            {
                throw new CultureNotFoundException(name, "Localization parameter is missing!");
            }

            return value;
        }

    }
}
