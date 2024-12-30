using HttpObservable.Models;
using System.Net.Http.Json;
using System.Reactive.Disposables;

namespace HttpObservable
{
    public class HttpObservable : BaseObservable
    {
        public readonly HttpClient _http;
        public HttpObservable(HttpClient http)
        {
            _http = http??throw new NullReferenceException(typeof(HttpClient).Name);
        }

       private IAsyncObservable<TDto> CreateRequest<TDto>(HttpRequestMessage request)
        {
            return CreateObservable<TDto>(async o =>
            {
                var response = await _http.SendAsync(request);                
                await HandleResponseAsync(response, o);
                return AsyncDisposable.Create(() => ValueTask.CompletedTask);
            });
        }

        public IAsyncObservable<TDto> PostAsJson<TDto, TPayload>(string url, TPayload data)
        {

            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Post, url, data);

            return CreateRequest<TDto>(request);
        }

        public IAsyncObservable<TDto> PostContent<TDto>(string url, HttpContent content)
        {
            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Post, url, content);

            return CreateRequest<TDto>(request);
        }

        public IAsyncObservable<TDto> PutAsJson<TDto, TPayload>(string url, TPayload data)
        {
            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Put, url, data);

            return CreateRequest<TDto>(request);
        }

        public IAsyncObservable<TDto> Delete<TDto>(string url)
        {
            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Delete, url);

            return CreateRequest<TDto>(request);
        }

        public IAsyncObservable<TDto> Get<TDto>(string url)
        {
            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Get, url);
            return CreateRequest<TDto>(request);
        }
    }
}
