using System;
using System.Threading.Tasks;
using System.Web.Services;
using MessageService_ASB.DeviceEventHelper;
namespace MessageService_ASB
{
    [WebService(Namespace = "http://tempuri.org/")]
    public class DeviceEventService : WebService
    {
        private readonly IDeviceEventSender _sender;
        public DeviceEventService()
        {
            string connectionString = System.Configuration.ConfigurationManager.AppSettings["ServiceBusConnectionString"];
            _sender = new DeviceEventSender(connectionString);
        }
        [WebMethod]
        public async Task<string> SendDeviceDataToImage(string airportCode, string orgWorkstation, string destWorkstation, string device, string data)
        {
            try
            {
                await _sender.SendDeviceEventAsync(airportCode, orgWorkstation, destWorkstation, device, data);
                return "Message sent successfully";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}