using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace Producer

{

    /// <summary>
    /// Represents a data object with a id (UUID), timestamp, service IP, and consumed status.
    /// Will be stored in a CosmosDB.
    /// </summary>
    public class Data
    {

        public string id { get; }
        public string Timestamp { get; }
        public string ServiceIp { get; }
        public bool Consumed { get; }

        public Data()
        {
            id = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now.ToString("U");
            ServiceIp = GetLocalIpAddress();
            Consumed = false;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        private string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

    }
    
}