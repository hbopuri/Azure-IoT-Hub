using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using Red.Common;

namespace Red.MessageProcessor
{
    class Program
    {
        private static string _iotHubConnectionString;
        private static string _storageConnectionString;
        private static string _hubName;
        private static string _storageContainerName;
        private static void Init()
        {
            var configuration = AppSetting.Load(Directory.GetCurrentDirectory());
            _iotHubConnectionString = configuration.GetConnectionString("IotHubConnectionString");
            _storageConnectionString = configuration.GetConnectionString("StorageConnectionString");
            _hubName = configuration.GetSection("constant")["hubName"];
            _storageContainerName = configuration.GetSection("constant")["storageContainerName"];
        }

        static async Task Main(string[] args)
        {
            Init();
            var consumerGroupName = PartitionReceiver.DefaultConsumerGroupName;
            var processor = new EventProcessorHost(_hubName, consumerGroupName, _iotHubConnectionString,
                _storageConnectionString, _storageContainerName);
            await processor.RegisterEventProcessorAsync<LoggingEventProcessor>();
            Console.WriteLine("Event Processor started, press enter to exit...");
            Console.ReadLine();
            await processor.UnregisterEventProcessorAsync();

        }
    }
}
