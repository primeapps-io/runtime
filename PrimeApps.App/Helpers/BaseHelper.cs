using System;

namespace PrimeApps.App.Helpers
{
    public interface IBaseHelper { }

    public class BaseHelper : IBaseHelper, IDisposable
    {
        public void Dispose()
        {
        }
    }
}
