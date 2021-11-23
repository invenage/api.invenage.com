using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvenageAPI.Models
{
    public class APIResponseModel<T>
    {
        public bool IsSuccess { get; set; }
        public string StatusCode { get; set; }
        public T Response { get; set; }
    }
}
