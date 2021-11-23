namespace InvenageAPI.Models
{
    public class NameData : DataModel
    {
        public string UserId { internal get; set; }
        public string Scope { get; set; }
        public string Salt { get; set; }
        public string DisplayName { get; set; }
        public bool UseSaltedName { get; set; }
    }
}
