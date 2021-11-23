using System.Collections.Generic;

namespace InvenageAPI.Models
{
    public class TokenData : DataModel
    {
        public string UserId { internal get; set; }
        public string TokenName { get; set; }
        public List<string> Scopes { get; set; }
        public long LastAccessTime { get; set; }
    }
}
