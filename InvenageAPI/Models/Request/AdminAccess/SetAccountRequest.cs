using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InvenageAPI.Models
{
    public class SetAccountRequest
    {
        [Required]
        [MaxLength(20)]
        public string Alias { get; set; }
        [Required]
        public string Passcode { get; set; }
    }
}
