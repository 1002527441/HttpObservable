using System.Text.Json;
using System.Text;

namespace HttpObservable
{
    public static class HttpRequestHelper
    {
        public static HttpRequestMessage CreateHttpRequest(HttpMethod method, string url, object? content = null)
        {
            var request = new HttpRequestMessage(method, url);

            if (content == null) return request;

            switch (content)
            {
                case HttpContent httpContent:
                    request.Content = httpContent;
                    break;
                default:
                    var json = JsonSerializer.Serialize(content);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    break;
            }


            return request;
        }

    }
}
