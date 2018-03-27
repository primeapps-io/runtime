using Elmah;
using System;
using System.Diagnostics;

namespace PrimeApps.App.Helpers
{
    class ErrorHandler
    {
        /// <summary>
        /// Logs error to elmah.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="details"></param>
        public static void LogError(Exception ex, string details = "")
        {
            ErrorLog errorLog = ErrorLog.GetDefault(null);
            Error err = new Error(ex);
            err.Message = string.Format("{0} {1}", details, err.Message);
            errorLog.Log(err);

            #if (DEBUG)
            throw ex;
            #endif
        }
    }
}
