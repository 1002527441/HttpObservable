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



        protected async Task HandleResponseAsync<TDto>(ApiResponse<TDto>? response, IAsyncObserver<TDto> observer)
        {
            if (response is null)
            {
                var errorMessage = $"Failed to deserialize response to {typeof(ApiResponse<TDto>).GetType().Name}";

                await observer.OnErrorAsync(new Exception(errorMessage));
            }
            else if (response.Succeeded)
            {
                await observer.OnNextAsync(response.Data!);
                await observer.OnCompletedAsync();
            }
            else
            {
                var errorMessage = response.Error!.Message;
                var ex = new Exception(errorMessage);
                await observer.OnErrorAsync(ex);
            }
        }

      
    }
}
