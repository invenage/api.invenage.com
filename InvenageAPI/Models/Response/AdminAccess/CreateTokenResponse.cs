using System.Collections.Generic;

namespace InvenageAPI.Models
{
    public class CreateTokenResponse
    {
        public List<string> Scope { get; set; }
        public string Token { get; set; }
    }
}