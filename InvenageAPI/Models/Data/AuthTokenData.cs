using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InvenageAPI.Models
{
    public class AuthTokenData : DataModel
    {
        public string TokenName { get; set; }
        [Required]
        public string Token { internal get; set; }
        public string ClientId { internal get; set; }
        public string UserId { get; set; }
        public List<string> Scopes { get; set; }
    }
}
