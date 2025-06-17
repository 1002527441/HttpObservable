using System.Net;

namespace HttpObservable.Models;

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
