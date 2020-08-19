using System.ComponentModel.DataAnnotations;

namespace CloudHeavenApi.Models
{
    public class AuthorizeRequest
    {
        [Required] public string accessToken { get; set; }

        [Required] public string clientToken { get; set; }
    }
}