using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudHeavenApi.Contexts;
using CloudHeavenApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloudHeavenApi.Services
{
    public interface AuthService
    {
        Task<TokenProfile> Authenticate(AuthenticateRequest request);

        Task<bool> Invalidate(AuthorizeRequest request);

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
        public string? Cause { get; set; }
    }
}
