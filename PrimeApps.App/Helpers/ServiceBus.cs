using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PrimeApps.App.Helpers
{
    public static class ServiceBus
    {
        static string connectionString = ConfigurationManager.AppSettings.Get("Microsoft.ServiceBus.ConnectionString");
        static MessagingFactory _factory;

        /// <summary>
        /// Cached queue clients, string key is the name of queue.
        /// </summary>
        static Dictionary<string, QueueClient> _queueClients = new Dictionary<string, QueueClient>();
        static Dictionary<string, MessageSender> _messageSenders = new Dictionary<string, MessageSender>();

        static MessagingFactory GetFactory()
        {
            if (_factory?.IsClosed ?? true)
            {
                _factory = MessagingFactory.CreateFromConnectionString(connectionString);
            }

            return _factory;
        }

        public static QueueClient GetQueueClient(string queueName)
        {
            var factory = GetFactory();
            QueueClient client;
            _queueClients.TryGetValue(queueName, out client);


            if (client?.IsClosed ?? true)
            {
                client = factory.CreateQueueClient(queueName);
                _queueClients.Add(queueName, client);
            }

            return client;
        }

        public static async Task<MessageSender> GetMessageSender(string queueName)
        {
            var factory = GetFactory();

            MessageSender client;
            _messageSenders.TryGetValue(queueName, out client);
            if (client?.IsClosed ?? true)
            {
                client = await factory.CreateMessageSenderAsync(queueName);
                _messageSenders.Add(queueName, client);
            }

            return client;
        }

        public static async Task<MessageReceiver> GetMessageReceiver(string queueName)
        {
            var factory = GetFactory();
            return await factory.CreateMessageReceiverAsync(queueName);
        }

        /// <summary>
        /// Sends a message to the queue.
        /// </summary>
        /// <param name="queueName">Name of the queue in the namespace of connection string.</param>
        /// <param name="message">A serializable object</param>
        /// <returns></returns>
        public static async Task SendMessage(string queueName, dynamic message, DateTime queueDate)
        {
            MessageSender sender = await GetMessageSender(queueName);
            BrokeredMessage bmsg = new BrokeredMessage(message);
            bmsg.ScheduledEnqueueTimeUtc = queueDate;
            await sender.SendAsync(bmsg);
        }

    }
}