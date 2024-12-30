namespace HttpObservable.Samples.Services
{
    public class HttpClientTokenHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 添加自定义逻辑，例如添加认证令牌
            request.Headers.Add("Authorization", "Bearer your-token");

            var response = await base.SendAsync(request, cancellationToken);

            // 处理响应，例如重试逻辑
            if (!response.IsSuccessStatusCode)
            {
                // 处理错误
            }

            return response;
        }
    }
}
