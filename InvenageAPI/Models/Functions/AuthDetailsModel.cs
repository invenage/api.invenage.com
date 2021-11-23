using System.Collections.Generic;

namespace InvenageAPI.Models
{
    public class AuthDetailsModel
    {
        public string GrantType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TokenName { get; set; }
        public string UserId { get; set; }
        public List<string> Scopes { get; set; }
    }
}
