using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Exceptions
{

    [Serializable]
    public class TenantNotFoundException : Exception
    {
        public TenantNotFoundException() { }
        public TenantNotFoundException(string message) : base(message) { }
        public TenantNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected TenantNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
