



namespace HttpObservable;

internal interface IHttpObservable
{
    IAsyncObservable<TDto> Delete<TDto>(string url);
    IAsyncObservable<TDto> Get<TDto>(string url);
    IAsyncObservable<TDto> PostAsJson<TDto, TPayload>(string url, TPayload data);
    IAsyncObservable<TDto> PostContent<TDto>(string url, HttpContent content);
    IAsyncObservable<TDto> PutAsJson<TDto, TPayload>(string url, TPayload data);
    IAsyncObservable<TDto> UploadFile<TDto>(string url, string filename, Stream stream);
    IAsyncObservable<IEnumerable<TDto>> UploadFiles<TDto>(string url, string[] filenames, Stream[] streams);
}