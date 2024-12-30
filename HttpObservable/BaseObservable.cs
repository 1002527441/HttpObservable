using HttpObservable.Models;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace HttpObservable
{
    public class BaseObservable
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
                var error = $"StatusCode:{response.StatusCode},{response.RequestMessage}";   
                var ex = new HttpRequestException(error);
                await observer.OnErrorAsync(ex);
            }
        }
      
    }
}
