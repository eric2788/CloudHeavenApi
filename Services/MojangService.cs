using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudHeavenApi.Models;
using Newtonsoft.Json;

namespace CloudHeavenApi.Services
{
    public class MojangService : AuthService
    {
        public static readonly string AuthApi = "https://authserver.mojang.com";

        private readonly HttpClient _client = new HttpClient();

        public async Task<TokenProfile> Authenticate(AuthenticateRequest request)
        {
            var content = JsonContent(request);
            var response = await _client.PostAsync($"{AuthApi}/authenticate", content);
            return await ToProfileAsync(response);
        }

        public async Task<bool> Invalidate(AuthorizeRequest request)
        {
            var content = JsonContent(request);
            var response = await _client.PostAsync($"{AuthApi}/invalidate", content);
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<TokenProfile> Refresh(AuthorizeRequest request)
        {
            var content = JsonContent(request);
            var response = await _client.PostAsync($"{AuthApi}/refresh", content);
            return await ToProfileAsync(response);
        }

        public static async Task<TokenProfile> ToProfileAsync(HttpResponseMessage response)
        {
            var str = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = JsonConvert.DeserializeObject<ErrorResponse>(str);
                throw new AuthException(error);
            }

            var authStructure = JsonConvert.DeserializeObject<MojangResponse>(str);
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

    class MojangResponse
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
