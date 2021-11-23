using Newtonsoft.Json;

namespace InvenageAPI.Models
{
    public class CorsData : DataModel
    {
        public string ClientId { get; set; }
        public string Origin { get; set; }
        public string ApiKey { internal get; set; }
        public string UserId { internal get; set; }
    }
}
