using System.ComponentModel.DataAnnotations;

namespace InvenageAPI.Models
{
    public class CORSAccessRequest
    {
        [Required]
        public string ClientId { get; set; }
        [Required]
        public string Origin { get; set; }
    }
}
