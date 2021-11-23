using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvenageAPI.Models
{
    public class AccountData : DataModel
    {
        public string Alias { get; set; }
        public string Passcode { get; set; }
        public string UserId { get; set; }
    }
}
