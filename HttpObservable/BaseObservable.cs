﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using HttpObservable.Models;
﻿﻿﻿﻿using System.Net;
﻿﻿﻿﻿using System.Text.Json;
﻿﻿﻿﻿using System.Reactive.Disposables;
﻿﻿﻿﻿using System.Reactive.Linq;
﻿﻿﻿﻿using Microsoft.Extensions.Logging;
﻿﻿﻿﻿using System.Net.Http.Headers;
﻿﻿﻿﻿using System.Collections.Concurrent;

﻿﻿﻿﻿namespace HttpObservable
﻿﻿﻿﻿{
﻿﻿﻿﻿    /// <summary>
﻿﻿﻿﻿    /// 重试策略类型
﻿﻿﻿﻿    /// </summary>
﻿﻿﻿﻿    public enum RetryPolicyType
﻿﻿﻿﻿    {
﻿﻿﻿﻿        /// <summary>
﻿﻿﻿﻿        /// 固定延迟重试
﻿﻿﻿﻿        /// </summary>
﻿﻿﻿﻿        FixedDelay,
        
﻿﻿﻿﻿        /// <summary>
﻿﻿﻿﻿        /// 指数退避重试
﻿﻿﻿﻿        /// </summary>
﻿﻿﻿﻿        ExponentialBackoff,
        
﻿﻿﻿﻿        /// <summary>
﻿﻿﻿﻿        /// 线性递增重试
﻿﻿﻿﻿        /// </summary>
﻿﻿﻿﻿        LinearBackoff,
        
﻿﻿﻿﻿        /// <summary>
﻿﻿﻿﻿        /// 自定义重试策略
﻿﻿﻿﻿        /// </summary>
﻿﻿﻿﻿        Custom
﻿﻿﻿﻿    }

﻿﻿﻿﻿    /// <summary>
﻿﻿﻿﻿    /// 提供可观察HTTP操作的基础功能
﻿﻿﻿﻿    /// </summary>
﻿﻿﻿﻿    public abstract class BaseObservable
﻿﻿﻿﻿    {
﻿﻿﻿﻿        private readonly JsonSerializerOptions? _jsonOptions;
﻿﻿﻿﻿        private readonly int _maxRetries;
﻿﻿﻿﻿        private readonly TimeSpan _initialRetryDelay;
﻿﻿﻿﻿        private readonly RetryPolicyType _retryPolicy;
﻿﻿﻿﻿        private readonly Func<int, TimeSpan>? _customRetryDelayFunc;
﻿﻿﻿﻿        private readonly ILogger? _logger;
﻿﻿﻿﻿        private readonly ConcurrentDictionary<string, (object Data, DateTime Expiry)> _cache;
﻿﻿﻿﻿        private readonly TimeSpan _defaultCacheExpiry;

﻿﻿﻿﻿        /// <summary>
﻿﻿﻿﻿        /// 初始化 <see cref="BaseObservable"/> 类的新实例
﻿﻿﻿﻿        /// </summary>
﻿﻿﻿﻿        /// <param name="jsonOptions">可选的JSON序列化选项</param>
﻿﻿﻿﻿        /// <param name="maxRetries">失败请求的最大重试次数</param>
﻿﻿﻿﻿        /// <param name="retryDelay">重试尝试之间的延迟</param>
﻿﻿﻿﻿        /// <param name="retryPolicy">重试策略类型</param>
﻿﻿﻿﻿        /// <param name="customRetryDelayFunc">自定义重试延迟函数</param>
﻿﻿﻿﻿        /// <param name="logger">用于日志记录的记录器</param>
﻿﻿﻿﻿        /// <param name="enableCaching">是否启用响应缓存</param>
﻿﻿﻿﻿        /// <param name="defaultCacheExpiry">默认缓存过期时间</param>
﻿﻿﻿﻿        protected BaseObservable(
﻿﻿﻿﻿            JsonSerializerOptions? jsonOptions = null,
﻿﻿﻿﻿            int maxRetries = 3,
﻿﻿﻿﻿            TimeSpan? retryDelay = null,
﻿﻿﻿﻿            RetryPolicyType retryPolicy = RetryPolicyType.FixedDelay,
﻿﻿﻿﻿            Func<int, TimeSpan>? customRetryDelayFunc = null,
﻿﻿﻿﻿            ILogger? logger = null,
﻿﻿﻿﻿            bool enableCaching = false,
﻿﻿﻿﻿            TimeSpan? defaultCacheExpiry = null)
﻿﻿﻿﻿        {
﻿﻿﻿﻿            _jsonOptions = jsonOptions ?? new JsonSerializerOptions
﻿﻿﻿﻿            {
﻿﻿﻿﻿                PropertyNameCaseInsensitive = true,
﻿﻿﻿﻿                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
﻿﻿﻿﻿                WriteIndented = false
﻿﻿﻿﻿            };
            
﻿﻿﻿﻿            _maxRetries = maxRetries;
﻿﻿﻿﻿            _initialRetryDelay = retryDelay ?? TimeSpan.FromSeconds(1);
﻿﻿﻿﻿            _retryPolicy = retryPolicy;
﻿﻿﻿﻿            _customRetryDelayFunc = customRetryDelayFunc;
﻿﻿﻿﻿            _logger = logger;
            
﻿﻿﻿﻿            if (enableCaching)
﻿﻿﻿﻿            {
﻿﻿﻿﻿                _cache = new ConcurrentDictionary<string, (object, DateTime)>();
﻿﻿﻿﻿                _defaultCacheExpiry = defaultCacheExpiry ?? TimeSpan.FromMinutes(5);
﻿﻿﻿﻿            }
﻿﻿﻿﻿            else
﻿﻿﻿﻿            {
﻿﻿﻿﻿                _cache = null!;
﻿﻿﻿﻿                _defaultCacheExpiry = TimeSpan.Zero;
﻿﻿﻿﻿            }
﻿﻿﻿﻿        }

        /// <summary>
        /// 从缓存中获取数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="data">输出数据</param>
        /// <returns>如果缓存命中返回true，否则返回false</returns>
        protected bool TryGetFromCache<T>(string cacheKey, out T? data)
        {
            data = default;
            
            if (_cache == null || string.IsNullOrEmpty(cacheKey))
                return false;
                
            if (_cache.TryGetValue(cacheKey, out var cacheEntry) && cacheEntry.Expiry > DateTime.UtcNow)
            {
                data = (T)cacheEntry.Data;
                _logger?.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 将数据添加到缓存
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="data">要缓存的数据</param>
        /// <param name="expiry">过期时间</param>
        protected void AddToCache<T>(string cacheKey, T data, TimeSpan? expiry = null)
        {
            if (_cache == null || string.IsNullOrEmpty(cacheKey) || data == null)
                return;
                
            var expiryTime = DateTime.UtcNow.Add(expiry ?? _defaultCacheExpiry);
            _cache[cacheKey] = (data, expiryTime);
            _logger?.LogDebug("Added to cache: {CacheKey}, expires at {ExpiryTime}", cacheKey, expiryTime);
        }
        
        /// <summary>
        /// 从缓存中移除数据
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        protected void RemoveFromCache(string cacheKey)
        {
            if (_cache == null || string.IsNullOrEmpty(cacheKey))
                return;
                
            _cache.TryRemove(cacheKey, out _);
            _logger?.LogDebug("Removed from cache: {CacheKey}", cacheKey);
        }
        
        /// <summary>
        /// 清除所有缓存
        /// </summary>
        protected void ClearCache()
        {
            if (_cache == null)
                return;
                
            _cache.Clear();
            _logger?.LogDebug("Cache cleared");
        }

        /// <summary>
        /// 从观察者工厂创建可观察序列
        /// </summary>
        /// <typeparam name="T">可观察序列的类型</typeparam>
        /// <param name="observerFactory">创建观察者的工厂函数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="cacheKey">缓存键，如果指定则启用缓存</param>
        /// <returns>可观察序列</returns>
        protected IAsyncObservable<T> CreateObservable<T>(
            Func<IAsyncObserver<T>, CancellationToken, ValueTask<IAsyncDisposable>> observerFactory,
            CancellationToken cancellationToken = default,
            string? cacheKey = null)
        {
            if (observerFactory == null) throw new ArgumentNullException(nameof(observerFactory));

            // 尝试从缓存获取
            if (!string.IsNullOrEmpty(cacheKey) && TryGetFromCache<T>(cacheKey, out var cachedData) && cachedData != null)
            {
                return AsyncObservable.Create<T>(async o =>
                {
                    await o.OnNextAsync(cachedData);
                    await o.OnCompletedAsync();
                    return AsyncDisposable.Create(() => ValueTask.CompletedTask);
                });
            }

            return AsyncObservable.Create<T>(async o =>
            {
                IAsyncDisposable? disposable = null;
                var retryCount = 0;
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        _logger?.LogDebug("Executing request{RetryInfo}", retryCount > 0 ? $" (retry {retryCount}/{_maxRetries})" : "");
                        disposable = await observerFactory(o, cts.Token);
                        
                        // 如果我们到达这里，说明请求成功
                        break;
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        _logger?.LogInformation("Request was cancelled by user");
                        await o.OnErrorAsync(new ApiException("Request was cancelled by user", 
                            HttpStatusCode.RequestTimeout, 
                            new OperationCanceledException("Request was cancelled by user", cancellationToken)));
                        break;
                    }
                    catch (Exception ex) when (ShouldRetry(ex, retryCount))
                    {
                        retryCount++;
                        var delay = CalculateRetryDelay(retryCount);
                        
                        _logger?.LogWarning(ex, "Request failed with retryable error. Retrying in {Delay}ms ({RetryCount}/{MaxRetries})", 
                            delay.TotalMilliseconds, retryCount, _maxRetries);
                            
                        try
                        {
                            await Task.Delay(delay, cts.Token);
                        }
                        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
                        {
                            _logger?.LogInformation("Retry was cancelled");
                            await o.OnErrorAsync(new ApiException("Retry was cancelled", 
                                HttpStatusCode.RequestTimeout,
                                new OperationCanceledException("Retry was cancelled", cancellationToken)));
                            break;
                        }
                        continue;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Request failed with non-retryable error");
                        await o.OnErrorAsync(new ApiException(ex.Message, 
                            ex is HttpRequestException httpEx ? httpEx.StatusCode ?? HttpStatusCode.InternalServerError : HttpStatusCode.InternalServerError,
                            ex));
                        break;
                    }
                }

                return disposable ?? AsyncDisposable.Create(() => 
                {
                    cts.Dispose();
                    return ValueTask.CompletedTask;
                });
            });
        }
        
        /// <summary>
        /// 计算重试延迟时间
        /// </summary>
        /// <param name="retryCount">当前重试次数</param>
        /// <returns>下一次重试的延迟时间</returns>
        protected TimeSpan CalculateRetryDelay(int retryCount)
        {
            return _retryPolicy switch
            {
                RetryPolicyType.FixedDelay => _initialRetryDelay,
                
                RetryPolicyType.ExponentialBackoff => TimeSpan.FromMilliseconds(
                    _initialRetryDelay.TotalMilliseconds * Math.Pow(2, retryCount - 1)),
                    
                RetryPolicyType.LinearBackoff => TimeSpan.FromMilliseconds(
                    _initialRetryDelay.TotalMilliseconds * retryCount),
                    
                RetryPolicyType.Custom => _customRetryDelayFunc?.Invoke(retryCount) ?? _initialRetryDelay,
                
                _ => _initialRetryDelay
            };
        }

        /// <summary>
        /// 处理HTTP响应并相应地通知观察者
        /// </summary>
        /// <typeparam name="TDto">响应反序列化的类型</typeparam>
        /// <param name="response">HTTP响应消息</param>
        /// <param name="observer">要通知的观察者</param>
        /// <returns>表示异步操作的任务</returns>
        protected async Task HandleResponseAsync<TDto>(
            HttpResponseMessage? response,
            IAsyncObserver<TDto> observer)
        {
            try
            {
                if (response == null)
                {
                    throw new ApiException("Response was null", HttpStatusCode.InternalServerError);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await DeserializeResponseAsync<TDto>(response);
                    await observer.OnNextAsync(result);
                    await observer.OnCompletedAsync();
                }
                else
                {
                    var error = await GetErrorDetailsAsync(response);
                    throw new ApiException(
                        error,
                        response.StatusCode);
                }
            }
            catch (ApiException)
            {
                throw; // 已经是ApiException，直接抛出
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message, HttpStatusCode.InternalServerError, ex);
            }
        }

﻿        /// <summary>
﻿        /// Deserializes the HTTP response content to the specified type.
﻿        /// </summary>
﻿        /// <typeparam name="T">The type to deserialize to.</typeparam>
﻿        /// <param name="response">The HTTP response message.</param>
﻿        /// <returns>The deserialized object.</returns>
﻿        protected virtual async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
﻿        {
﻿            if (response == null) throw new ArgumentNullException(nameof(response));

﻿            try
﻿            {
﻿                var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
﻿                if (result == null)
﻿                {
﻿                    throw new JsonException($"Failed to deserialize response to {typeof(T).Name}");
﻿                }
﻿                return result;
﻿            }
﻿            catch (JsonException ex)
﻿            {
﻿                var content = await response.Content.ReadAsStringAsync();
﻿                throw new JsonException(
﻿                    $"Failed to deserialize response to {typeof(T).Name}. Content: {content}",
﻿                    ex);
﻿            }
﻿        }

﻿        /// <summary>
﻿        /// Gets detailed error information from a failed HTTP response.
﻿        /// </summary>
﻿        /// <param name="response">The HTTP response message.</param>
﻿        /// <returns>A detailed error message.</returns>
﻿        protected virtual async Task<string> GetErrorDetailsAsync(HttpResponseMessage response)
﻿        {
﻿            if (response == null) throw new ArgumentNullException(nameof(response));

﻿            var content = await response.Content.ReadAsStringAsync();
            
﻿            try
﻿            {
﻿                var error = await response.Content.ReadFromJsonAsync<Err>(_jsonOptions);
﻿                if (error?.Message != null)
﻿                {
﻿                    return $"Status: {response.StatusCode}, Message: {error.Message}";
﻿                }
﻿            }
﻿            catch
﻿            {
﻿                // If we can't deserialize to Err, we'll use the raw content
﻿            }

﻿            return $"Status: {response.StatusCode}, URL: {response.RequestMessage?.RequestUri}, Content: {content}";
﻿        }

        /// <summary>
        /// 确定是否应该对给定异常进行重试
        /// </summary>
        /// <param name="ex">要检查的异常</param>
        /// <param name="retryCount">当前重试次数</param>
        /// <returns>如果应该重试则返回true，否则返回false</returns>
        protected virtual bool ShouldRetry(Exception ex, int retryCount)
        {
            if (retryCount >= _maxRetries) 
            {
                _logger?.LogInformation("Maximum retry attempts ({MaxRetries}) reached", _maxRetries);
                return false;
            }

            var shouldRetry = ex switch
            {
                HttpRequestException httpEx => httpEx.StatusCode switch
                {
                    HttpStatusCode.ServiceUnavailable => true,  // 503
                    HttpStatusCode.GatewayTimeout => true,      // 504
                    HttpStatusCode.RequestTimeout => true,      // 408
                    HttpStatusCode.BadGateway => true,          // 502
                    HttpStatusCode.TooManyRequests => true,     // 429
                    HttpStatusCode.InternalServerError => true,  // 500
                    _ => false
                },
                TimeoutException => true,
                SocketException => true,
                IOException => true,
                TaskCanceledException tce => tce.CancellationToken.IsCancellationRequested == false, // 只有在不是用户取消的情况下重试
                OperationCanceledException oce => oce.CancellationToken.IsCancellationRequested == false, // 只有在不是用户取消的情况下重试
                JsonException => false, // JSON解析错误通常不是临时的
                _ => false
            };

            if (shouldRetry)
            {
                _logger?.LogDebug("Exception {ExceptionType} is retryable", ex.GetType().Name);
            }
            else
            {
                _logger?.LogDebug("Exception {ExceptionType} is not retryable", ex.GetType().Name);
            }

            return shouldRetry;
        }

        /// <summary>
        /// 创建HTTP请求的默认头部
        /// </summary>
        /// <returns>HTTP请求头集合</returns>
        protected virtual HttpRequestHeaders CreateDefaultHeaders()
        {
            var headers = new HttpRequestMessage().Headers;
            headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            headers.UserAgent.Add(new ProductInfoHeaderValue("HttpObservable", "1.0"));
            return headers;
        }

        /// <summary>
        /// 生成缓存键
        /// </summary>
        /// <param name="url">请求URL</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>缓存键</returns>
        protected virtual string GenerateCacheKey(string url, object? parameters = null)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var key = url.ToLowerInvariant();
            
            if (parameters != null)
            {
                var paramJson = JsonSerializer.Serialize(parameters, _jsonOptions);
                key = $"{key}_{paramJson}";
            }

            return key;
        }
﻿    }
﻿}