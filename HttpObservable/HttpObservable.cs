﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿pu﻿﻿﻿﻿using HttpObservable.Models;
﻿using System.Net.Http.Json;
﻿using System.Reactive.Disposables;
﻿using System.Text.Json;

﻿namespace HttpObservable
﻿{
﻿    /// <summary>
﻿    /// Provides HTTP operations that return observable sequences.
﻿    /// </summary>
﻿    public class HttpObservable : BaseObservable,
﻿        IHttpObservable,
﻿        IHttpObservableAsync
﻿    {
﻿        private readonly HttpClient _http;
﻿        private readonly JsonSerializerOptions? _jsonOptions;
﻿        private readonly ICacheProvider? _cacheProvider;

﻿        /// <summary>
﻿        /// 缓存提供程序接口
﻿        /// </summary>
﻿        public interface ICacheProvider
﻿        {
﻿            /// <summary>
﻿            /// 尝试从缓存获取数据
﻿            /// </summary>
﻿            /// <typeparam name="T">数据类型</typeparam>
﻿            /// <param name="key">缓存键</param>
﻿            /// <param name="value">获取到的数据</param>
﻿            /// <returns>如果获取成功返回true，否则返回false</returns>
﻿            bool TryGetValue<T>(string key, out T? value);

﻿            /// <summary>
﻿            /// 添加数据到缓存
﻿            /// </summary>
﻿            /// <typeparam name="T">数据类型</typeparam>
﻿            /// <param name="key">缓存键</param>
﻿            /// <param name="value">要缓存的数据</param>
﻿            /// <param name="expiry">缓存过期时间</param>
﻿            void Set<T>(string key, T value, TimeSpan? expiry = null);

﻿            /// <summary>
﻿            /// 从缓存中移除数据
﻿            /// </summary>
﻿            /// <param name="key">缓存键</param>
﻿            void Remove(string key);
﻿        }

﻿        /// <summary>
﻿        /// 初始化HttpObservable类的新实例
﻿        /// </summary>
﻿        /// <param name="http">用于请求的HTTP客户端</param>
﻿        /// <param name="jsonOptions">可选的JSON序列化选项</param>
﻿        /// <param name="cacheProvider">可选的缓存提供程序</param>
﻿        /// <exception cref="ArgumentNullException">当http为null时抛出</exception>
﻿        public HttpObservable(
﻿            HttpClient http, 
﻿            JsonSerializerOptions? jsonOptions = null,
﻿            ICacheProvider? cacheProvider = null)
﻿        {
﻿            _http = http ?? throw new ArgumentNullException(nameof(http));
﻿            _jsonOptions = jsonOptions;
﻿            _cacheProvider = cacheProvider;
﻿        }

        /// <summary>
        /// 创建可观察的HTTP请求
        /// </summary>
        /// <typeparam name="TDto">响应数据的类型</typeparam>
        /// <param name="request">HTTP请求消息</param>
        /// <param name="cancellationToken">可选的取消令牌</param>
        /// <param name="cacheKey">缓存键，如果指定则启用缓存</param>
        /// <param name="cacheExpiry">缓存过期时间</param>
        /// <returns>产生响应数据的可观察序列</returns>
        /// <exception cref="ArgumentNullException">当request为null时抛出</exception>
        private IAsyncObservable<TDto> CreateRequest<TDto>(
            HttpRequestMessage request, 
            CancellationToken cancellationToken = default,
            string? cacheKey = null,
﻿                try
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 尝试从缓存获取
            if (!string.IsNullOrEmpty(cacheKey) && TryGetFromCache<TDto>(cacheKey, out var cachedData) && cachedData != null)
            {
                return AsyncObservable.Create<TDto>(async o =>
                {
                    await o.OnNextAsync(cachedData);
﻿                {
                    return AsyncDisposable.Create(() => ValueTask.CompletedTask);
                });
            }

            return CreateObservable<TDto>(async o =>
            {
                try
                {
                    var response = await _http.SendAsync(request, cancellationToken);
                    await HandleResponseAsync<TDto>(response, o, cacheKey, cacheExpiry);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    await o.OnErrorAsync(new OperationCanceledException("HTTP请求已取消", cancellationToken));
                }
                catch (Exception ex)
                {
                    await o.OnErrorAsync(ex);
                }
                
                return AsyncDisposable.Create(() => ValueTask.CompletedTask);
            });
        }

        /// <summary>
        /// 尝试从缓存获取数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="data">获取到的数据</param>
        /// <returns>如果获取成功返回true，否则返回false</returns>
        private bool TryGetFromCache<T>(string cacheKey, out T? data)
        {
            data = default;
            
            if (_cacheProvider == null || string.IsNullOrEmpty(cacheKey))
            {
                return false;
            }

            return _cacheProvider.TryGetValue(cacheKey, out data);
        }

        /// <summary>
        /// 添加数据到缓存
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="data">要缓存的数据</param>
        /// <param name="expiry">缓存过期时间</param>
        private void AddToCache<T>(string cacheKey, T data, TimeSpan? expiry = null)
        {
            if (_cacheProvider != null && !string.IsNullOrEmpty(cacheKey) && data != null)
            {
                _cacheProvider.Set(cacheKey, data, expiry);
            }
        }

        /// <summary>
        /// 从缓存中移除数据
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        public void RemoveFromCache(string cacheKey)
        {
            if (_cacheProvider != null && !string.IsNullOrEmpty(cacheKey))
            {
                _cacheProvider.Remove(cacheKey);
            }
        }

        /// <summary>
        /// 清除所有缓存数据
        /// </summary>
        public void ClearCache()
        {
            if (_cacheProvider is ICacheProvider clearableCache)
            {
                clearableCache.Clear();
            }
        }

        /// <summary>
        /// 处理HTTP响应
        /// </summary>
        /// <typeparam name="TDto">响应数据的类型</typeparam>
        /// <param name="response">HTTP响应消息</param>
        /// <param name="observer">异步观察者</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheExpiry">缓存过期时间</param>
        /// <returns>完成的任务</returns>
        private async Task HandleResponseAsync<TDto>(
            HttpResponseMessage response, 
            IAsyncObserver<TDto> observer,
            string? cacheKey = null,
            TimeSpan? cacheExpiry = null)
        {
            try
            {
                response.EnsureSuccessStatusCode();

                if (typeof(TDto) == typeof(HttpResponseMessage))
                {
                    await observer.OnNextAsync((TDto)(object)response);
                    await observer.OnCompletedAsync();
                    return;
                }

                if (response.Content == null)
                {
                    await observer.OnNextAsync(default!);
                    await observer.OnCompletedAsync();
                    return;
                }

                if (typeof(TDto) == typeof(string))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    await observer.OnNextAsync((TDto)(object)content);
                    await observer.OnCompletedAsync();
                    
                    if (!string.IsNullOrEmpty(cacheKey))
                    {
                        AddToCache(cacheKey, content, cacheExpiry);
                    }
                    return;
                }

                if (typeof(TDto) == typeof(byte[]))
                {
                    var content = await response.Content.ReadAsByteArrayAsync();
                    await observer.OnNextAsync((TDto)(object)content);
                    await observer.OnCompletedAsync();
                    
                    if (!string.IsNullOrEmpty(cacheKey))
                    {
                        AddToCache(cacheKey, content, cacheExpiry);
                    }
                    return;
                }

                if (typeof(TDto) == typeof(Stream))
                {
                    var content = await response.Content.ReadAsStreamAsync();
                    await observer.OnNextAsync((TDto)(object)content);
                    await observer.OnCompletedAsync();
                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<TDto>(_jsonOptions);
                
                if (result != null)
                {
                    await observer.OnNextAsync(result);
                    
                    if (!string.IsNullOrEmpty(cacheKey))
                    {
                        AddToCache(cacheKey, result, cacheExpiry);
                    }
                }
                else
                {
                    await observer.OnNextAsync(default!);
                }
                
                await observer.OnCompletedAsync();
            }
            catch (Exception ex)
            {
                await observer.OnErrorAsync(ex);
            }
        }

        /// <summary>
        /// 发送带查询参数的GET请求
        /// </summary>
        /// <typeparam name="TDto">响应数据的类型</typeparam>
        /// <param name="url">请求的URL</param>
        /// <param name="queryParams">查询参数字典</param>
        /// <param name="cancellationToken">可选的取消令牌</param>
        /// <returns>产生响应数据的可观察序列</returns>
        /// <exception cref="ArgumentException">当url无效时抛出</exception>
        public IAsyncObservable<TDto> GetWithQuery<TDto>(
            string url, 
            IDictionary<string, string?> queryParams, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL不能为空或空白", nameof(url));

            var uriBuilder = new UriBuilder(url);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            
            foreach (var param in queryParams)
            {
                if (param.Value != null)
                {
                    query[param.Key] = param.Value;
                }
            }
            
            uriBuilder.Query = query.ToString();
            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Get, uriBuilder.ToString());
            return CreateRequest<TDto>(request, cancellationToken);
        }

        /// <summary>
        /// 发送HEAD请求
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="cancellationToken">可选的取消令牌</param>
        /// <returns>产生HTTP响应消息的可观察序列</returns>
        /// <exception cref="ArgumentException">当url无效时抛出</exception>
        public IAsyncObservable<HttpResponseMessage> Head(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL不能为空或空白", nameof(url));

            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Head, url);
            return CreateRequest<HttpResponseMessage>(request, cancellationToken);
        }

        /// <summary>
        /// 发送OPTIONS请求
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="cancellationToken">可选的取消令牌</param>
        /// <returns>产生HTTP响应消息的可观察序列</returns>
        /// <exception cref="ArgumentException">当url无效时抛出</exception>
        public IAsyncObservable<HttpResponseMessage> Options(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL不能为空或空白", nameof(url));

            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Options, url);
            return CreateRequest<HttpResponseMessage>(request, cancellationToken);
        }

        /// <summary>
        /// 发送带表单数据的POST请求
        /// </summary>
        /// <typeparam name="TDto">响应数据的类型</typeparam>
        /// <param name="url">请求的URL</param>
        /// <param name="formData">表单数据字典</param>
        /// <param name="cancellationToken">可选的取消令牌</param>
        /// <returns>产生响应数据的可观察序列</returns>
        /// <exception cref="ArgumentException">当url无效时抛出</exception>
        public IAsyncObservable<TDto> PostFormData<TDto>(
            string url,
            IDictionary<string, string> formData,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL不能为空或空白", nameof(url));
            if (formData == null) throw new ArgumentNullException(nameof(formData));

            var content = new FormUrlEncodedContent(formData);
            return PostContent<TDto>(url, content, cancellationToken);
        }

        /// <summary>
        /// Sends a POST request with JSON content.
﻿        /// </summary>
﻿        /// <typeparam name="TDto">The type of the response data.</typeparam>
﻿        /// <typeparam name="TPayload">The type of the request payload.</typeparam>
﻿        /// <param name="url">The URL for the request.</param>
﻿        /// <param name="data">The data to send in the request body.</param>
﻿        /// <param name="cancellationToken">Optional cancellation token.</param>
﻿        /// <returns>An observable sequence that yields the response data.</returns>
﻿        /// <exception cref="ArgumentException">Thrown when url is invalid.</exception>
﻿        public IAsyncObservable<TDto> PostAsJson<TDto, TPayload>(string url, TPayload data, CancellationToken cancellationToken = default)
﻿        {
﻿            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));

﻿            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Post, url, data, _jsonOptions);
﻿            return CreateRequest<TDto>(request, cancellationToken);
﻿        }

﻿        /// <summary>
﻿        /// Sends a POST request with custom HTTP content.
﻿        /// </summary>
﻿        /// <typeparam name="TDto">The type of the response data.</typeparam>
﻿        /// <param name="url">The URL for the request.</param>
﻿        /// <param name="content">The content to send in the request body.</param>
﻿        /// <param name="cancellationToken">Optional cancellation token.</param>
﻿        /// <returns>An observable sequence that yields the response data.</returns>
﻿        /// <exception cref="ArgumentException">Thrown when url is invalid.</exception>
﻿        /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
﻿        public IAsyncObservable<TDto> PostContent<TDto>(string url, HttpContent content, CancellationToken cancellationToken = default)
﻿        {
﻿            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));
﻿            if (content == null) throw new ArgumentNullException(nameof(content));

﻿            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Post, url, content);
﻿            return CreateRequest<TDto>(request, cancellationToken);
﻿        }

﻿        /// <summary>
﻿        /// Sends a PUT request with JSON content.
﻿        /// </summary>
﻿        /// <typeparam name="TDto">The type of the response data.</typeparam>
﻿        /// <typeparam name="TPayload">The type of the request payload.</typeparam>
﻿        /// <param name="url">The URL for the request.</param>
﻿        /// <param name="data">The data to send in the request body.</param>
﻿        /// <param name="cancellationToken">Optional cancellation token.</param>
﻿        /// <returns>An observable sequence that yields the response data.</returns>
﻿        /// <exception cref="ArgumentException">Thrown when url is invalid.</exception>
﻿        public IAsyncObservable<TDto> PutAsJson<TDto, TPayload>(string url, TPayload data, CancellationToken cancellationToken = default)
﻿        {
﻿            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));

﻿            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Put, url, data, _jsonOptions);
﻿            return CreateRequest<TDto>(request, cancellationToken);
﻿        }

﻿        /// <summary>
﻿        /// Sends a PUT request with custom HTTP content.
﻿        /// </summary>
﻿        /// <typeparam name="TDto">The type of the response data.</typeparam>
﻿        /// <param name="url">The URL for the request.</param>
﻿        /// <param name="content">The content to send in the request body.</param>
﻿        /// <param name="cancellationToken">Optional cancellation token.</param>
﻿        /// <returns>An observable sequence that yields the response data.</returns>
﻿        /// <exception cref="ArgumentException">Thrown when url is invalid.</exception>
﻿        /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
﻿        public IAsyncObservable<TDto> PutContent<TDto>(string url, HttpContent content, CancellationToken cancellationToken = default)
﻿        {
﻿            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));
﻿            if (content == null) throw new ArgumentNullException(nameof(content));

﻿            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Put, url, content);
﻿            return CreateRequest<TDto>(request, cancellationToken);
﻿        }

﻿        /// <summary>
﻿        /// Sends a PATCH request with JSON content.
﻿        /// </summary>
﻿        /// <typeparam name="TDto">The type of the response data.</typeparam>
﻿        /// <typeparam name="TPayload">The type of the request payload.</typeparam>
﻿        /// <param name="url">The URL for the request.</param>
﻿        /// <param name="data">The data to send in the request body.</param>
﻿        /// <param name="cancellationToken">Optional cancellation token.</param>
﻿        /// <returns>An observable sequence that yields the response data.</returns>
﻿        /// <exception cref="ArgumentException">Thrown when url is invalid.</exception>
﻿        public IAsyncObservable<TDto> PatchAsJson<TDto, TPayload>(string url, TPayload data, CancellationToken cancellationToken = default)
﻿        {
﻿            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));

﻿            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Patch, url, data, _jsonOptions);
﻿            return CreateRequest<TDto>(request, cancellationToken);
﻿        }

﻿        /// <summary>
﻿        /// Sends a DELETE request.
﻿        /// </summary>
﻿        /// <typeparam name="TDto">The type of the response data.</typeparam>
﻿        /// <param name="url">The URL for the request.</param>
﻿        /// <param name="cancellationToken">Optional cancellation token.</param>
﻿        /// <returns>An observable sequence that yields the response data.</returns>
﻿        /// <exception cref="ArgumentException">Thrown when url is invalid.</exception>
﻿        public IAsyncObservable<TDto> Delete<TDto>(string url, CancellationToken cancellationToken = default)
﻿        {
﻿            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));

﻿            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Delete, url);
﻿            return CreateRequest<TDto>(request, cancellationToken);
﻿        }

﻿        /// <summary>
﻿        /// Sends a DELETE request with JSON content.
﻿        /// </summary>
﻿        /// <typeparam name="TDto">The type of the response data.</typeparam>
﻿        /// <typeparam name="TPayload">The type of the request payload.</typeparam>
﻿        /// <param name="url">The URL for the request.</param>
﻿        /// <param name="data">The data to send in the request body.</param>
﻿        /// <param name="cancellationToken">Optional cancellation token.</param>
﻿        /// <returns>An observable sequence that yields the response data.</returns>
﻿        /// <exception cref="ArgumentException">Thrown when url is invalid.</exception>
﻿        public IAsyncObservable<TDto> DeleteWithBody<TDto, TPayload>(string url, TPayload data, CancellationToken cancellationToken = default)
﻿        {
﻿            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));

﻿            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Delete, url, data, _jsonOptions);
﻿            return CreateRequest<TDto>(request, cancellationToken);
﻿        }

﻿        /// <summary>
﻿        /// Sends a GET request.
﻿        /// </summary>
﻿        /// <typeparam name="TDto">The type of the response data.</typeparam>
﻿        /// <param name="url">The URL for the request.</param>
﻿        /// <param name="cancellationToken">Optional cancellation token.</param>
﻿        /// <returns>An observable sequence that yields the response data.</returns>
﻿        /// <exception cref="ArgumentException">Thrown when url is invalid.</exception>
﻿        public IAsyncObservable<TDto> Get<TDto>(string url, CancellationToken cancellationToken = default)
﻿        {
﻿            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));

﻿            var request = HttpRequestHelper.CreateHttpRequest(HttpMethod.Get, url);
﻿            return CreateRequest<TDto>(request, cancellationToken);
﻿        }

        /// <summary>
        /// Sends a custom HTTP request.
        /// </summary>
        /// <typeparam name="TDto">The type of the response data.</typeparam>
        /// <param name="method">The HTTP method for the request.</param>
        /// <param name="url">The URL for the request.</param>
        /// <param name="content">Optional content to include in the request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>An observable sequence that yields the response data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when method is null.</exception>
        /// <exception cref="ArgumentException">Thrown when url is invalid.</exception>
        public IAsyncObservable<TDto> SendRequest<TDto>(
            HttpMethod method, 
            string url, 
            object? content = null, 
            CancellationToken cancellationToken = default)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));

            var request = HttpRequestHelper.CreateHttpRequest(method, url, content, _jsonOptions);
            return CreateRequest<TDto>(request, cancellationToken);
        }

        /// <summary>
        /// 上传文件到指定URL
        /// </summary>
        /// <param name="url">上传目标URL</param>
        /// <param name="filePath">要上传的文件路径</param>
        /// <param name="formData">额外的表单数据</param>
        /// <param name="progress">进度报告回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>上传响应</returns>
        public IAsyncObservable<HttpResponseMessage> UploadFileAsync(
            string url,
            string filePath,
            IDictionary<string, string>? formData = null,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            return CreateObservable(async (observer, ct) =>
            {
                try
                {
                    using var content = new MultipartFormDataContent();
                    
                    // 添加文件内容
                    var fileStream = File.OpenRead(filePath);
                    var fileContent = new ProgressableStreamContent(
                        fileStream,
                        progress,
                        cancellationToken);
                    
                    content.Add(fileContent, "file", Path.GetFileName(filePath));

                    // 添加表单数据
                    if (formData != null)
                    {
                        foreach (var item in formData)
                        {
                            content.Add(new StringContent(item.Value), item.Key);
                        }
                    }

                    // 创建请求
                    var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = content
                    };

                    // 发送请求
                    var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                    await observer.OnNextAsync(response);
                    await observer.OnCompletedAsync();
                }
                catch (Exception ex)
                {
                    await observer.OnErrorAsync(new ApiException(ex.Message, HttpStatusCode.InternalServerError, ex));
                }
            }, cancellationToken);
        }

        private class ProgressableStreamContent : StreamContent
        {
            private readonly Stream _stream;
            private readonly IProgress<double>? _progress;
            private readonly CancellationToken _cancellationToken;

            public ProgressableStreamContent(
                Stream stream,
                IProgress<double>? progress,
                CancellationToken cancellationToken) : base(stream)
            {
                _stream = stream;
                _progress = progress;
                _cancellationToken = cancellationToken;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            {
                var buffer = new byte[8192];
                var totalLength = _stream.Length;
                var uploaded = 0L;

                while (true)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    
                    var length = await _stream.ReadAsync(buffer, 0, buffer.Length, _cancellationToken);
                    if (length <= 0) break;

                    await stream.WriteAsync(buffer, 0, length, _cancellationToken);
                    uploaded += length;

                    _progress?.Report((double)uploaded / totalLength);
                }
            }
        }
    }

        /// <summary>
        /// 内存缓存提供程序实现
        /// </summary>
        public class MemoryCacheProvider : ICacheProvider
        {
            private readonly ConcurrentDictionary<string, (object value, DateTimeOffset expiry)> _cache = new();

            public bool TryGetValue<T>(string key, out T? value)
            {
                value = default;
                
                if (_cache.TryGetValue(key, out var cached) && cached.expiry > DateTimeOffset.Now)
                {
                    value = (T)cached.value;
                    return true;
                }
                
                return false;
            }

            public void Set<T>(string key, T value, TimeSpan? expiry = null)
            {
                var expiryTime = expiry.HasValue 
                    ? DateTimeOffset.Now.Add(expiry.Value) 
                    : DateTimeOffset.MaxValue;
                
                _cache[key] = (value!, expiryTime);
            }

            public void Remove(string key)
            {
                _cache.TryRemove(key, out _);
            }

            public void Clear()
            {
                _cache.Clear();
            }
        }
﻿}