using System.Text.Json.Serialization;

namespace HttpObservable.Models
{
    /// <summary>
    /// Represents a standardized API response.
    /// </summary>
    /// <typeparam name="T">The type of data contained in the response.</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the request was successful.
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        /// <summary>
        /// Gets or sets the error details if the request was not successful.
        /// </summary>
        [JsonPropertyName("error")]
        public Err? Error { get; set; }

        /// <summary>
        /// Creates a successful response with data.
        /// </summary>
        /// <param name="data">The response data.</param>
        /// <param name="message">Optional success message.</param>
        /// <returns>A new ApiResponse instance.</returns>
        public static ApiResponse<T> CreateSuccess(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        /// <summary>
        /// Creates a failed response with an error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="error">Optional detailed error information.</param>
        /// <returns>A new ApiResponse instance.</returns>
        public static ApiResponse<T> CreateError(string message, Err? error = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = error
            };
        }
    }
}