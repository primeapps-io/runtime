using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Messaging;

namespace PrimeApps.App.Jobs.Messaging
{
    public abstract class MessageClient
    {
        public abstract Task<bool> Process(MessageDTO messageQueueItem, UserItem appUser);

        /// <summary>
        /// Formats messages by replacing actual field values.
        /// </summary>
        /// <param name="messageFields"></param>
        /// <param name="messageText"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public string FormatMessage(IList<string> messageFields, string messageText, JObject record)
        {
            var formattedMessage = messageText;

            foreach (var messageField in messageFields)
            {
                formattedMessage = formattedMessage.Replace($"{{{messageField}}}", !record[messageField].IsNullOrEmpty() ? record[messageField].ToString() : "");
            }

            return formattedMessage;
        }
    }
}
