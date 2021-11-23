namespace InvenageAPI.Models
{
    public class LogData : DataModel
    {
        public string TraceId { get; set; }
        public string CategoryName { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
    }
}
