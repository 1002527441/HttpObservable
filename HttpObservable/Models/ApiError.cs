using System.Net;

namespace HttpObservable.Models;

public class ApiResponse<TData>
{
    public int Code { get; set; }
    public bool Succeeded { get; set; }
    public TData? Data { get; set; }
    public ApiError? Error { get; set; }
    public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
}

public class PagedList<TEntity>
{
    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 页容量
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总条数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 当前页集合
    /// </summary>
    public List<TEntity> Items { get; set; } = new List<TEntity>();

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevPages { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPages { get; set; }
}



public class ApiError
{
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; } 
    public string jsonData { get; set; }      
}


public class ApiException : Exception
{
    public ApiError Error { get; set; }
    public ApiException(ApiError error) : base(error.Message)
    {
        Error = error;
    }
    public ApiException(string message, ApiError error) : base(message)
    {
        Error = error;
    }
    public ApiException(string message, Exception innerException, ApiError error) : base(message, innerException)
    {
        Error = error;
    }
}
