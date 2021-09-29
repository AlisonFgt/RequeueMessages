using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

namespace RequeueMessages
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = "Endpoint=sb://....";
            string queueName = "service.v1.action.other";

            // since ServiceBusClient implements IAsyncDisposable we create it with "await using"
            await using var client = new ServiceBusClient(connectionString);

            // create the receiver
            ServiceBusReceiver receiver = client.CreateReceiver(queueName + "/$DeadLetterQueue");

            // create the sender
            ServiceBusSender sender = client.CreateSender(queueName);

            object body = null;
            do
            {
                // receive message
                ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(new TimeSpan(0,0,20));

                if (receivedMessage != null)
                {
                    body = receivedMessage.Body;
                    Console.WriteLine("Send message:");
                    Console.WriteLine(body);

                    // send the message
                    await sender.SendMessageAsync(new ServiceBusMessage(receivedMessage.Body));

                    // remove deadLetter message
                    await receiver.CompleteMessageAsync(receivedMessage);
                }
                else
                    body = null;

            } while (body != null);

            Console.WriteLine("Exit program...");
        }
    }
}
