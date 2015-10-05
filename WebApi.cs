using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Launch__
{
    internal class WebApi
    {
        private const string SessionUrl = "https://passport.nexoneu.com/en/";
        private const string LoginUrl = "https://passport.nexoneu.com/Service/Authentication.asmx/Login";

        private const string UserAgent =
            "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.104 Safari/537.36";

        /// <summary>
        ///     Attempts to login using the Authentication API of MapleStory
        /// </summary>
        /// <param name="userId">The userId thats used to authenticate</param>
        /// <param name="password">The password that's used to authenticate</param>
        /// <returns>Return the AuthCookie set by the API</returns>
        /// <exception cref="Exception">The API failed to respond, or there is no AuthCookie received</exception>
        public static async Task<Cookie> LoginAsync(string userId, string password)
        {
            try
            {
                var handler = new HttpClientHandler { UseCookies = true };
                using (var c = new HttpClient(handler))
                {
                    SetHeaders(c.DefaultRequestHeaders);


                    HttpResponseMessage response = await c.GetAsync(new Uri(SessionUrl));
                    response.EnsureSuccessStatusCode();

                    string json = String.Format("{{\"account\":{{\"userId\":\"{0}\",\"password\":\"{1}\",\"accessedGame\":\"maplestory\",\"captcha\":\"\",\"isSaveID\":false}}}}", userId, password);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await c.PostAsync(new Uri(LoginUrl), content);
                    response.EnsureSuccessStatusCode();

                    return handler.CookieContainer.GetCookies(response.RequestMessage.RequestUri)
                        .Cast<Cookie>()
                        .FirstOrDefault(cookie => cookie.Name.Equals("NPP", StringComparison.CurrentCultureIgnoreCase));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in LoginAsync:\n{0}", e);
                throw;
            }
        }

        /// <summary>
        ///     Resets the headers to make any request to the Nexon Api look legit
        /// </summary>
        /// <param name="headers">The <see cref="HttpRequestHeaderCollection" /> that gets modified</param>
        private static void SetHeaders(HttpRequestHeaders headers)
        {
            headers.Clear();

            headers.Accept.ParseAdd("application/json, text/javascript, */*; q=0.01");
            headers.AcceptEncoding.ParseAdd("gzip,deflate");
            headers.AcceptLanguage.ParseAdd("en-GB,en-us;q=0.8,en;q=0.6");
            headers.UserAgent.ParseAdd(UserAgent);
            headers.Connection.TryParseAdd("keep-alive");
            headers.Host = new Uri(LoginUrl).Host;
            headers.Add("X-Requested-With", "XMLHttpRequest");
        }
    }
}
