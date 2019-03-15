using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Red.Common;

namespace Red.Telemetry.Manager
{
    class Program
    {
        private static string _serviceConnectionString;

        private static void Init()
        {
            var configuration = AppSetting.Load(Directory.GetCurrentDirectory());
            _serviceConnectionString = configuration.GetConnectionString("ServiceConnectionString");
        }
        // ReSharper disable once UnusedParameter.Local
        static async Task Main(string[] args)
        {
            Init();
            Console.WriteLine("Initializing LCM Manager!");
            
            var serviceClient = ServiceClient.CreateFromConnectionString(_serviceConnectionString);
            var feedbackTask = ReceiveFeedBackAsync(serviceClient);
            while (true)
            {
                Console.WriteLine("Which device do you wish to send a message to?");
                Console.Write("> ");
                var deviceId = Console.ReadLine();
                await SendCloudToDeviceMessageAsync(serviceClient, deviceId);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static async Task ReceiveFeedBackAsync(ServiceClient serviceClient)
        {
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null)
                {
                    continue;
                }

                foreach (var record in feedbackBatch.Records)
                {
                    var messageId = record.OriginalMessageId;
                    var statusCode = record.StatusCode;
                    Console.WriteLine($"Feedback for message '{messageId}', status code: {statusCode}.");
                }

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }

        private static async Task SendCloudToDeviceMessageAsync(ServiceClient serviceClient, string deviceId)
        {
            //Console.WriteLine("Pick a option\n'o' to POWER ON\n'c' to POWER OFF");
            Console.WriteLine($"Type a message you want to send to device: {deviceId}");
            Console.Write("> ");
            var payload = Console.ReadLine();
            var commandMessage = new Message(Encoding.ASCII.GetBytes(payload))
            {
                MessageId = Guid.NewGuid().ToString(),
                Ack = DeliveryAcknowledgement.Full,
                ExpiryTimeUtc = DateTime.UtcNow.AddSeconds(10)
            };
            //var input = Console.ReadKey().KeyChar;
            ////Console.WriteLine();
            //var message = string.Empty;
            //switch (char.ToLower(input))
            //{
            //    case 'o':
            //        message = "Power on -"+DateTime.Now.Ticks;
            //        break;
            //    case 'c':
            //        message = "Power off -" + DateTime.Now.Ticks;
            //        break;
            //}

            //var command = new CloudCommand
            //{
            //    Message = message,
            //    Signal = Signal.Something
            //};
            //var payloadString = JsonConvert.SerializeObject(command);
            //var commandMessage = new Message(Encoding.ASCII.GetBytes(payloadString))
            //{
            //    MessageId = Guid.NewGuid().ToString(),
            //    Ack = DeliveryAcknowledgement.Full,
            //    ExpiryTimeUtc = DateTime.UtcNow.AddSeconds(10)
            //};
            await serviceClient.SendAsync(deviceId, commandMessage);
        }
    }
}
