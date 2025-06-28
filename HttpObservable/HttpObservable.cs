using HttpObservable.Models;
using System.Net.Http.Json;
using System.Reactive.Disposables;

namespace HttpObservable
{
    public abstract class HttpObservable : BaseObservable, IHttpObservable
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

        public virtual IAsyncObservable<TDto> PostAsJson<TDto, TPayload>(string url, TPayload data)
        {

            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Post, url, data);

            return CreateRequest<TDto>(request);
        }

        public virtual IAsyncObservable<TDto> PostContent<TDto>(string url, HttpContent content)
        {
            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Post, url, content);

            return CreateRequest<TDto>(request);
        }

        public virtual IAsyncObservable<TDto> PutAsJson<TDto, TPayload>(string url, TPayload data)
        {
            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Put, url, data);

            return CreateRequest<TDto>(request);
        }

        public virtual IAsyncObservable<TDto> Delete<TDto>(string url)
        {
            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Delete, url);

            return CreateRequest<TDto>(request);
        }

        public virtual IAsyncObservable<TDto> Get<TDto>(string url)
        {
            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Get, url);
            return CreateRequest<TDto>(request);
        }

        public virtual IAsyncObservable<TDto> UploadFile<TDto>(string url, string filename, Stream stream) 
        {            
            var content = HttpRequestHelper.CreateUploadFileContent(stream, filename);
            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Post, url, content);
            return CreateRequest<TDto>(request);
        }

        public virtual IAsyncObservable<IEnumerable<TDto>> UploadFiles<TDto>(string url, string[] filenames, Stream[] streams)
        {
            var fileName = Path.GetFileName(url);
            var content = HttpRequestHelper.CreateUploadFilesContent(streams, filenames);
            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Post, url, content);
            return CreateRequest<IEnumerable<TDto>>(request);
        }
    }
}
