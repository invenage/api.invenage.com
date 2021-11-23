using System.ComponentModel.DataAnnotations;

namespace InvenageAPI.Models
{
    public class ChangeNameRequest
    {
        [Required]
        public string Scope { get; set; }
        public string Salt { get; set; }
        public string DisplayName { get; set; }
        public bool? UseSaltedName { get; set; }
    }
}
