using System;
using System.Threading.Tasks;
using CloudHeavenApi.Models;

namespace CloudHeavenApi.Services
{
    public interface IAuthService
    {
        Task<TokenProfile> Authenticate(AuthenticateRequest request);

        Task<bool> Invalidate(AuthorizeRequest request);

        Task<TokenProfile> Validate(AuthorizeRequest request);

        Task<TokenProfile> Refresh(AuthorizeRequest request);
    }

    public class TokenProfile
    {
        public Guid UUID { get; set; }
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public string ClientToken { get; set; }
    }

    public class AuthException : Exception
    {
        public readonly ErrorResponse ErrorResponse;

        public AuthException(ErrorResponse response)
        {
            ErrorResponse = response;
        }
    }

    public class ErrorResponse
    {
        public string Error { get; set; }
        public string ErrorMessage { get; set; }
        public string CauseFrom { get; set; }
        public string[] Cause { get; set; }
    }
}