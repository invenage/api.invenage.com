using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvenageAPI.Models
{
    public class CreateTokenRequest
    {
        [Required]
        public string TokenName { get; set; }

        [Required]
        public List<string> Scope { get; set; }
    }
}
