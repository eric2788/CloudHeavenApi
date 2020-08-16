using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudHeavenApi.Models
{
    public class AuthorizeRequest
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string ClientToken { get; set; }
    }
}
