using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeApps.Model.Common.Resources
{
public sealed class ResourceNameAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string _resourceName;

        // This is a positional argument
        public ResourceNameAttribute(string resourceName)
        {
            this._resourceName = resourceName;
        }

        public string ResourceName
        {
            get { return _resourceName; }
        }
    }
}
