using Sentry;
using System;

namespace PrimeApps.Admin.Helpers
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
            SentrySdk.CaptureException(ex);
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