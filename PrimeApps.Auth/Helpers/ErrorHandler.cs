using Sentry;
using System;
using System.Diagnostics;

namespace PrimeApps.Auth.Helpers
{
    class ErrorHandler
    {
        /// <summary>
        /// Logs error to sentry.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void LogError(Exception ex, string message = "")
        {
            Exception exception = (Exception)Activator.CreateInstance(ex.GetType(), string.Format("{0} {1}", ex.Message, message));
            
            SentrySdk.CaptureException(exception);

            if (Debugger.IsAttached)
                throw ex;
            
        }

        /// <summary>
        /// Logs message to sentry.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="level"></param>
        public static void LogMessage(string message = "", Sentry.Protocol.SentryLevel level = Sentry.Protocol.SentryLevel.Info)
        {
            SentrySdk.CaptureMessage(message, level);
        }
    }
}
