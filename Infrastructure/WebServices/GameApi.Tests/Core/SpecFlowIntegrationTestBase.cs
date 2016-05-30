using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Providers;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using AFT.UGS.Core.Messages.Players;

using Newtonsoft.Json;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.GameApi.Tests.Core
{
    public abstract class SpecFlowIntegrationTestBase : SpecFlowUnitTestBase
    {
        protected const string DefaultPlayertIp = "50.67.208.24";
        protected const string DefaultBrand = "138";
        protected static readonly IJsonSerializationProvider Json = new JsonSerializationProvider();

        [StepArgumentTransformation(@"will (.*)be")]
        public bool WillNotBe(string word)
        {
            return word.Trim() != "not";
        }
        protected static string NewStringId { get { return Guid.NewGuid().ToString(); } }
        protected static IGameRepository GetOrCreateGamesDb()
        {
            return GetOrCreate<IGameRepository>(
                        SR.GsiDb, () => new GameRepository());
        }

        protected static Task<HttpResponseMessage> JsonHttp(HttpMethod method, string url, object content = null, IDictionary<string,string> headers = null )
        {
            var http = GetOrCreate(SR.http, () => new HttpClient());
            var req = new HttpRequestMessage(method, url);
            req.Headers.Remove(SR.HeaderAccept);
            req.Headers.Add(SR.HeaderAccept, "application/json");
            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                if (content != null)
                {
                    req.Content = new StringContent(Json.SerializeToString(content));
                    req.Content.Headers.Remove(SR.HeaderContentType);
                    req.Content.Headers.Add(SR.HeaderContentType, "application/json");
                }
            }
            else if (content != null)
            {
                throw new InvalidOperationException("Unexpected to have content for method " + method);
            }
            if (headers != null)
            {
                foreach (var h in headers)
                {
                    req.Headers.Remove(h.Key);
                    req.Headers.Add(h.Key, h.Value);
                }
            }
            return http.SendAsync(req);
        }

        protected static Task<T> JsonPost<T>(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttp<T>(HttpMethod.Post, url, content, headers);
        }
        protected static Task<T> JsonPut<T>(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttp<T>(HttpMethod.Put, url, content, headers);
        }
        protected static Task<T> JsonDelete<T>(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttp<T>(HttpMethod.Delete, url, content, headers);
        }
        protected static Task<T> JsonGet<T>(string url, IDictionary<string,string> headers = null)
        {
            return JsonHttp<T>(HttpMethod.Get, url, null, headers);
        }

        protected static Task<T> JsonPostSecure<T>(string url, string accessToken, object content = null, IDictionary<string, string> headers = null)
        {
            SetAccessToken(accessToken, ref headers);
            return JsonHttp<T>(HttpMethod.Post, url, content, headers);
        }
        protected static Task<T> JsonPutSecure<T>(string url, string accessToken, object content = null, IDictionary<string, string> headers = null)
        {
            SetAccessToken(accessToken, ref headers);
            return JsonHttp<T>(HttpMethod.Put, url, content, headers);
        }
        protected static Task<T> JsonDeleteSecure<T>(string url, string accessToken, object content = null, IDictionary<string, string> headers = null)
        {
            SetAccessToken(accessToken, ref headers);
            return JsonHttp<T>(HttpMethod.Delete, url, content, headers);
        }
        protected static Task<T> JsonGetSecure<T>(string url, string accessToken, IDictionary<string, string> headers = null)
        {
            SetAccessToken(accessToken, ref headers);
            return JsonHttp<T>(HttpMethod.Get, url, null, headers);
        }

        private static void SetAccessToken(string accessToken, ref IDictionary<string, string> headers)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            headers.Add(SR.HeaderAuthorization, "Bearer " + accessToken);
        }

        protected static Task<dynamic> JsonPost(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttpDynamic(HttpMethod.Post, url, content, headers);
        }
        protected static Task<dynamic> JsonPut(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttpDynamic(HttpMethod.Put, url, content, headers);
        }
        protected static Task<dynamic> JsonDelete(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttpDynamic(HttpMethod.Delete, url, content, headers);
        }
        protected static Task<dynamic> JsonGet(string url, IDictionary<string,string> headers = null)
        {
            return JsonHttpDynamic(HttpMethod.Get, url, null, headers);
        }

        protected static IDictionary<string, string> Headers(params string[] args)
        {
            return args == null || args.Length == 0 
                    ? null 
                    : args
                        .Select(a => a.Split(new[] {':'}, 2))
                        .ToDictionary(arr => arr[0].Trim(), arr => arr.Length < 2 ? "" : arr[1].Trim());
        }

        private static async Task<dynamic> JsonHttpDynamic(HttpMethod method, string url, object content, IDictionary<string, string> headers = null)
        {
            var res = await JsonHttp(method, url, content, headers);
            var json = await res.Content.ReadAsStringAsync();
            try
            {
                return Json.DeserializeAsDynamic(json);
            }
            catch (JsonReaderException jre)
            {
                throw new InvalidDataException("Cannot parse JSON", jre);
            } 
        }
        private static async Task<T> JsonHttp<T>(HttpMethod method, string url, object content, IDictionary<string,string> headers = null)
        {
            var res = await JsonHttp(method, url, content, headers);
            if ( res.StatusCode != HttpStatusCode.OK && res.StatusCode != HttpStatusCode.InternalServerError) 
                throw new WebException("Web exception with status code: " + res.StatusCode);
            var json = await res.Content.ReadAsStringAsync();
            try
            {
                return Json.DeserializeFromString<T>(json);
            }
            catch (JsonReaderException jre)
            {
                throw new InvalidDataException("Cannot parse JSON", jre);
            } 
        }

        protected static Task<HttpResponseMessage> FormHttp(HttpMethod method, string url, FormUrlEncodedContent content = null, IDictionary<string, string> headers = null)
        {
            var http = GetOrCreate(SR.http, () => new HttpClient());
            var req = new HttpRequestMessage(method, url);

            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                if (content != null)
                {
                    req.Content = content;
                    req.Content.Headers.Remove(SR.HeaderContentType);
                    req.Content.Headers.Add(SR.HeaderContentType, "application/x-www-form-urlencoded");
                }
            }
            else if (content != null)
            {
                throw new InvalidOperationException("Unexpected to have content for method " + method);
            }
            if (headers != null)
            {
                foreach (var h in headers)
                {
                    req.Headers.Remove(h.Key);
                    req.Headers.Add(h.Key, h.Value);
                }
            }
            return http.SendAsync(req);
        }

        private static async Task<dynamic> FormHttpDynamic(HttpMethod method, string url, FormUrlEncodedContent content, IDictionary<string, string> headers = null)
        {
            var res = await FormHttp(method, url, content, headers);
            var json = await res.Content.ReadAsStringAsync();
            try
            {
                return Json.DeserializeAsDynamic(json);
            }
            catch (JsonReaderException jre)
            {
                throw new InvalidDataException("Cannot parse JSON", jre);
            } 

        }

        protected static Task<dynamic> FormPost(string url, FormUrlEncodedContent content = null, IDictionary<string, string> headers = null)
        {
            return FormHttpDynamic(HttpMethod.Post, url, content, headers);
        }


        protected async Task<string> AuthorizePlayer(string player, string password)
        {
            var result =
                await JsonPostSecure<AuthorizePlayerResponse>(
                        Config.GameApiUrl + "api/player/authorize",
                        Get<string>(SR.accesstoken),
                        new AuthorizePlayerRequest
                        {
                            username = player,
                            ipaddress = DefaultPlayertIp
                        });

            return result.authtoken;
        }

        protected async Task<string> GetAccessTokenFor(string apiPath, string clientId, string clientSecret)
        {
            dynamic result =
                await FormPost(Config.GameApiUrl + apiPath,
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"client_id", clientId},
                        {"client_secret", clientSecret},
                        {"grant_type", "client_credentials"},
                        {"scope", "bets players"}
                    }));

            return result.access_token;
        }

        protected Uri BuildUri(string relativePath)
        {
            return new Uri(new Uri(Config.GameApiUrl), relativePath);
        }

        protected string GetFullApiPath(string relativePath)
        {
            return BuildUri(relativePath).ToString();
        }

        protected void SetGameProvider(string clientId, string clientSecret)
        {
            var gsiDb = GetOrCreateGamesDb();
            var gpId = (from gp in gsiDb.GameProviderConfigurations
                             where gp.AuthorizationClientId == clientId && gp.AuthorizationSecret == clientSecret
                             select gp.GameProviderId).Single();

            Set(SR.GameProviderId, gpId);
        }
    }
}