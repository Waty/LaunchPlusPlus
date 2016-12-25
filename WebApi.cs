using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Launch__
{
    public class NexonToken
    {
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string RefreshToken { get; set; }
    }

    public static class WebApi
    {
        private const string LoginUrl = "https://api.nexon.net/auth/login";
        private static readonly Uri BaseUri = new Uri("https://api.nexon.net");

        static WebApi()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        public static Task<NexonToken> RefreshToken(NexonToken token)
        {
            return Login(new {token.RefreshToken, AllowUnverified = true});
        }

        public static Task<NexonToken> Login(string email, string password)
        {
            return Login(new {UserId = email, UserPw = password, AllowUnverified = true});
        }

        private static async Task<NexonToken> Login(object data)
        {
            var filter = new HttpClientHandler();
            using (var c = new HttpClient(filter))
            {
                c.DefaultRequestHeaders.UserAgent.ParseAdd("NexonLauncher.nxl-16.10.03-150-6b2c4c1");

                string json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await c.PostAsync(new Uri(LoginUrl), content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                string responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeAnonymousType(responseStr, new
                {
                    Token = "",
                    RefreshToken = "",
                    AuthToken = "",
                    ExpiresIn = 0,
                    Error = ""
                });

                return new NexonToken
                {
                    RefreshToken = result.RefreshToken,
                    ExpirationDate = DateTime.Now.AddSeconds(result.ExpiresIn),
                    Token = result.Token
                };
            }
        }

        public static async Task<TResult> GetApi<TResult>(Uri uri, string token, TResult result)
        {
            var filter = new HttpClientHandler();
            using (var c = new HttpClient(filter))
            {
                c.DefaultRequestHeaders.UserAgent.ParseAdd("NexonLauncher.nxl-16.10.03-150-6b2c4c1");
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(token)));
                filter.CookieContainer.Add(BaseUri, new Cookie("nxtk", token) {Domain = ".nexon.net", Path = "/"});

                string s = await c.GetStringAsync(uri).ConfigureAwait(false);
                return JsonConvert.DeserializeAnonymousType(s, result);
            }
        }

        public static async Task<string> GetPassport(string token)
        {
            var desc = new {AuthToken = "", Passport = "", UserNo = 0};
            var res = await GetApi(new Uri(BaseUri, "/users/me/passport"), token, desc).ConfigureAwait(false);
            return res.Passport;
        }
    }
}