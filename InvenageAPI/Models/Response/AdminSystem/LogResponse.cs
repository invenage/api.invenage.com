using System.Collections.Generic;

namespace InvenageAPI.Models
{
    public class LogResponse
    {
        public IEnumerable<LogData> Logs { get; set; }
    }
}
