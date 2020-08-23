using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudHeavenApi.Models;
using CloudHeavenApi.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CloudHeavenApi.Implementation
{
    public class MojangService : IAuthService
    {
        public static readonly string AuthApi = "https://authserver.mojang.com";
        private readonly ICacheService<Identity> _cacheService;

        private readonly HttpClient _client = new HttpClient();

        private readonly ILogger<IAuthService> _logger;

        public MojangService(ILogger<IAuthService> logger, ICacheService<Identity> cacheService)
        {
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<TokenProfile> Authenticate(AuthenticateRequest request)
        {
            var payload = new Dictionary<string, object>
            {
                ["agent"] = new Dictionary<string, object>
                {
                    ["name"] = "Minecraft",
                    ["version"] = 1
                },
                ["username"] = request.UserName,
                ["password"] = request.Password,
                ["requestUser"] = true
            };
            var content = JsonContent(payload);
            var response = await _client.PostAsync($"{AuthApi}/authenticate", content);
            return await ToProfileAsync(response);
        }

        public async Task<bool> Invalidate(AuthorizeRequest request)
        {
            var content = JsonContent(request);
            var response = await _client.PostAsync($"{AuthApi}/invalidate", content);
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                _cacheService.RemoveItem(request.clientToken);
                return true;
            }

            return false;
        }

        public async Task<TokenProfile> Validate(AuthorizeRequest request)
        {
            var content = JsonContent(request);
            _logger.LogDebug($"validate json: {JsonConvert.SerializeObject(request)}");
            var response = await _client.PostAsync($"{AuthApi}/validate", content);
            _logger.LogDebug($"response: {response.StatusCode}");
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                var error = JsonConvert.DeserializeObject<ErrorResponse>(await response.Content.ReadAsStringAsync());
                throw new AuthException(error);
            }

            if (!_cacheService.TryGetItem(request.clientToken, out var id))
            {
                await Invalidate(request);
                throw new AuthException(new ErrorResponse
                {
                    Error = "Invalid Session",
                    ErrorMessage = "Please ReLogin"
                });
            }

            return new TokenProfile
            {
                AccessToken = request.accessToken,
                ClientToken = request.clientToken,
                UUID = id.UUID,
                UserName = id.UserName
            };
        }

        public async Task<TokenProfile> Refresh(AuthorizeRequest request)
        {
            var content = JsonContent(request);
            var response = await _client.PostAsync($"{AuthApi}/refresh", content);
            return await ToProfileAsync(response);
        }

        private async Task<TokenProfile> ToProfileAsync(HttpResponseMessage response)
        {
            var str = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = JsonConvert.DeserializeObject<ErrorResponse>(str);
                throw new AuthException(error);
            }

            var authStructure = JsonConvert.DeserializeObject<MojangResponse>(str);
            _cacheService.SetItem(authStructure.ClientToken, new Identity
            {
                UserName = authStructure.SelectedProfile.Name,
                UUID = authStructure.SelectedProfile.UUID
            });
            return new TokenProfile
            {
                AccessToken = authStructure.AccessToken,
                ClientToken = authStructure.ClientToken,
                UUID = authStructure.SelectedProfile.UUID,
                UserName = authStructure.SelectedProfile.Name
            };
        }

        public static StringContent JsonContent(object json)
        {
            return new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
        }
    }

    internal class MojangResponse
    {
        public string AccessToken { get; set; }
        public string ClientToken { get; set; }

        public SelectedProfileStructure SelectedProfile { get; set; }

        public class SelectedProfileStructure
        {
            public string Name { get; set; }
            public string Id { get; set; }

            public Guid UUID => Guid.ParseExact(Id, "N");
        }
    }
}