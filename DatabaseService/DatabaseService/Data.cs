using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace DatabaseService

{
    public class Data
    {
        public string id { get; set; }
        public string Timestamp { get; set; }
        public string ServiceIp { get; set; }
        public bool Consumed { get; set; }
    }
    
}