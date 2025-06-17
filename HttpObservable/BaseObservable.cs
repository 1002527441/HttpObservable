using HttpObservable.Models;
using System.Net.Http.Json;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpObservable
{
    public abstract class BaseObservable
    {

        protected IAsyncObservable<T> CreateObservable<T>(Func<IAsyncObserver<T>, ValueTask<IAsyncDisposable>> observerFactory)
        {

            var observable = AsyncObservable.Create<T>(async o =>
            {
                IAsyncDisposable? disposable = null;

                try
                {
                    disposable = await observerFactory(o);
                }
                catch (Exception ex)
                {
                    await o.OnErrorAsync(ex);
                }

                return disposable ?? AsyncDisposable.Create(() => ValueTask.CompletedTask);
            });

            return observable;
        }



        protected async Task HandleResponseAsync<TDto>(HttpResponseMessage? response, IAsyncObserver<TDto> observer)
        {
            if (response is null)
            {
                var errorMessage = $"Failed to deserialize response to {typeof(TDto).GetType().Name}";

                await observer.OnErrorAsync(new Exception(errorMessage));
            }
            else if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TDto>();
                await observer.OnNextAsync(result!);
                await observer.OnCompletedAsync();
            }
            else
            {
                var apiError = new ApiError();              
                apiError.jsonData = await response.Content.ReadAsStringAsync();    
                apiError.StatusCode = response.StatusCode;
                var ex  = new ApiException(apiError);
                await observer.OnErrorAsync(ex);
            }
        }
      
    }
}
