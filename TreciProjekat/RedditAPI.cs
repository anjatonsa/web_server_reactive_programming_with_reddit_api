using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreciProjekat
{
    public class RedditAPI
    {
        public async Task<string> getAccessToken(string clientId, string secret)
        {
            var client = new HttpClient();

            var tokenRequestBody = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
            };

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{clientId}:{secret}")));
            client.DefaultRequestHeaders.Add("User-Agent", "Getting the access token for the project.");

            var tokenResponse = await client.PostAsync("https://www.reddit.com/api/v1/access_token", new FormUrlEncodedContent(tokenRequestBody));
            var responseContent = await tokenResponse.Content.ReadAsStringAsync();

            var jsonResponse=JObject.Parse(responseContent);
            var accessToken = jsonResponse["access_token"]?.ToString();
            return accessToken;
        }

    }
}
