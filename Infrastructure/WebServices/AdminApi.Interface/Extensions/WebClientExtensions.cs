using System.IO;
using System.Net;
using System.Text;
using System.Web;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Proxy;
using Newtonsoft.Json;

namespace AFT.RegoV2.AdminApi.Interface.Extensions
{
    public static class WebClientExtensions
    {
       public static void SetToken(this WebClient client, string scheme, string token)
        {
            client.Headers[HttpRequestHeader.Authorization] = scheme + token;
        }

        public static void SetBearerToken(this WebClient client, string token)
        {
            client.SetToken("Bearer ", token);
        }

        public static void SetHeaders(this WebClient client)
        {
            client.Headers[HttpRequestHeader.Accept] = "application/json";
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
        }

        public static void HandleWebException(WebException e)
        {
            var webResponse = e.Response as HttpWebResponse;
            if (webResponse != null)
            {
                HttpWebResponse response = webResponse;

                if (response.StatusCode != HttpStatusCode.OK)
                {

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        var apiException =
                            new AdminApiException
                            {
                                ErrorMessage = "Method not found"
                            };
                        throw new AdminApiProxyException(apiException, response.StatusCode);
                    }

                    var responseText = "";

                    var responseStream = response.GetResponseStream();

                    if (responseStream != null)
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            responseText = reader.ReadToEnd();
                        }
                    }

                    if (responseText.Contains(HttpStatusCode.Forbidden.ToString().ToLower()))
                    {
                        throw new HttpException(403, "Access forbidden");
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var apiException = JsonConvert.DeserializeObject<AdminApiException>(responseText);
                        throw new AdminApiProxyException(apiException, response.StatusCode);
                    }
                    else
                    {
                        var apiException =
                            new AdminApiException
                            {
                                ErrorMessage = "Unhandled exception: " + responseText
                            };
                        throw new AdminApiProxyException(apiException, response.StatusCode);
                    }
                }
            }
        }

        public static TResponse SecurePostAsJson<TRequest, TResponse>(this WebClient client, string token, string url, TRequest request)
        {
            client.SetHeaders();
            client.Encoding = Encoding.UTF8;

            if (token != null)
            {
                client.SetBearerToken(token);
            }

            try
            {
                var data = JsonConvert.SerializeObject(request);

                var response = client.UploadString(url, data);

                return JsonConvert.DeserializeObject<TResponse>(response);
            }

            catch (WebException e)
            {
                HandleWebException(e);
            }

            return default(TResponse);
        }

        public static T SecureGet<T>(this WebClient client, string token, string url, string query = "")
        {
            client.SetHeaders();
            client.Encoding = Encoding.UTF8;

            if (token != null)
            {
                client.SetBearerToken(token);
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(query))
                    url += "?" + query;

                var response = client.DownloadString(url);

                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (WebException e)
            {
                HandleWebException(e);
            }

            return default(T);
        }
    }
}
