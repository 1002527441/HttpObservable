public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; set; }
    /// <summary>
    public ApiException(string message) : base(message) { }
}