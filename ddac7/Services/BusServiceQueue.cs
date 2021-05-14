using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ddac7.Services
{
    public class BusServiceQueue
    {
        const string ServiceBusConnectionString = "Endpoint=sb://clinicappointment.servicebus.windows.net/;" +
            "SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAZ+/oYPQFohQ9O2ZEoHopo0MAEgib8IgAjN5iEnNpM=";
        const string QueueName = "testing";
        private static IQueueClient queueClient;
        public static List<string> items;
        public static async Task SendQueueMsg(String msg)
        {

            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            // Send messages.
            await SendMessagesAsync(msg);

            await queueClient.CloseAsync();
        }
        static async Task SendMessagesAsync(string msg)
        {
            try
            {
                string messageBody = msg;
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                await queueClient.SendAsync(message);

            }
            catch (Exception exception)
            {

            }
        }


    }
}
