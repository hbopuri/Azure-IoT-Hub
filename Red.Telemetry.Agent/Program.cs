using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Red.Common;

namespace Red.Telemetry.Agent
{
    class Program
    {
        private static string _deviceConnectionString;

        // ReSharper disable once UnusedParameter.Local
        static async Task Main(string[] args)
        {
            
            Init();

            Console.WriteLine("Initializing LCM Agent!");
            var device = DeviceClient.CreateFromConnectionString(_deviceConnectionString);
            await device.OpenAsync();
            // ReSharper disable once UnusedVariable
            var receiveEventsTask = ReceiveEventsAsync(device);
            Console.WriteLine("Device is connected!");
            await UpdateTwinAsync(device);

            var randomReading = new Random();
            var quitRequested = false;
            while (!quitRequested)
            {
                Console.WriteLine("Press 's' to push random sensor data to cloud or 'q' to exit");
                var input = Console.ReadKey().KeyChar;
                Console.WriteLine();
                switch (char.ToLower(input))
                {
                    case 's':

                        break;
                    case 'q':
                        quitRequested = true;
                        break;
                }

                var reading = new Sensor
                {
                    Comment = "",
                    ReadingDateTimeUtc = DateTime.UtcNow,
                    Reading = randomReading.NextDouble(),
                    StatusCode = (int) StatusType.Active
                };
                var message = new Message(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(reading)));
                await device.SendEventAsync(message);
                Console.WriteLine("Message sent to the cloud");
            }
        }

        private static void Init()
        {
            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);

            //IConfigurationRoot configuration = builder.Build();
            var configuration = AppSetting.Load(Directory.GetCurrentDirectory());
            _deviceConnectionString = configuration.GetConnectionString("DeviceConnectionString");
        }

        private static async Task UpdateTwinAsync(DeviceClient device)
        {
            var twinProperties = new TwinCollection {["connectionType"] = "GSM", ["connectionStrength"] = "WEAK"};
            await device.UpdateReportedPropertiesAsync(twinProperties);
        }

        private static async Task ReceiveEventsAsync(DeviceClient device)
        {
            while (true)
            {
                var message = await device.ReceiveAsync();
                if (message == null)
                {
                    continue;
                }

                var messageBody = message.GetBytes();
                var payload = Encoding.ASCII.GetString(messageBody);
                Console.WriteLine($"Received message from cloud: '{payload}'");
                //try
                //{
                //    var cloudCommand = JsonConvert.DeserializeObject<CloudCommand>(payload);
                //    if (cloudCommand == null || cloudCommand.Signal == Signal.Empty)
                //    {
                //        await device.RejectAsync(message);
                //    }
                //    else
                //    {
                //        await device.CompleteAsync(message);
                //    }
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine(e.Message);
                //    await device.RejectAsync(message);
                //}
                await device.CompleteAsync(message);
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}