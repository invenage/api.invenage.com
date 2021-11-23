using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvenageAPI.Models
{
    public class AuthRequestModel
    {
        public string Response_Type { get; set; }
        public string Client_Id { get; set; }
        public string Redirect_Uri { get; set; }
        public string Scope { get; set; }
        public string State { get; set; }
    }
}
