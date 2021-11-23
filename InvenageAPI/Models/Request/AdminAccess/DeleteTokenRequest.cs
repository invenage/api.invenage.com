using System.ComponentModel.DataAnnotations;

namespace InvenageAPI.Models
{
    public class DeleteTokenRequest
    {
        [Required]
        public string TokenName { get; set; }
    }
}
