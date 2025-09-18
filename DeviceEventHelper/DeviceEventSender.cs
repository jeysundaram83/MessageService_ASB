using Azure.Messaging.ServiceBus;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MessageService_ASB.DeviceEventHelper
{

    public interface IDeviceEventSender
    {
        Task SendDeviceEventAsync(string airportCode, string orgWorkstation, string destWorkstation, string device, string data);

    }

    public class DeviceEventSender : IDeviceEventSender
    {

        private readonly string _connectionString;
        public DeviceEventSender(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SendDeviceEventAsync(string airportCode, string orgWorkstation, string destWorkstation, string device, string data)
        {
            int length = 44;
            string line1 = data.Length >= length ? data.Substring(0, length) : data;
            string line2 = data.Length > length ? data.Substring(length) : string.Empty;
            string formattedData = line1 + "\n" + line2;
            string topicName = $"as.tib.go.deviceevent.{airportCode.ToLower()}.topic";
            string firstLine = $"EventType=\"2\" OrgWorkstation=\"{orgWorkstation}\" DestWorkstation=\"{destWorkstation}\" CreationTime=\"{DateTime.Now:M/d/yyyy h:mm:ss tt}\" Device=\"{device}\"";

            string secondLine = $"Data=\"{formattedData}\"";
            string messageBody = firstLine + "\n" + secondLine;
            // Disable Diagnostic-Id / Activity propagation

            using (var listener = new ActivityListener
            {
                ShouldListenTo = source => false,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.None
            })
            {

                ActivitySource.AddActivityListener(listener);
                ServiceBusClient client = new ServiceBusClient(_connectionString,
                    new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });
                ServiceBusSender sender = client.CreateSender(topicName);
                ServiceBusMessage message = new ServiceBusMessage(messageBody)
                {
                    //ContentType = "text/plain"
                };

                message.ApplicationProperties.Remove("Diagnostic-Id");

                message.ApplicationProperties.Remove("Correlation-Id");

                message.ApplicationProperties.Remove("Request-Id");

                await sender.SendMessageAsync(message);

            }

        }

    }

}