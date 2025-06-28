﻿using System.Text.Json.Serialization;

﻿namespace HttpObservable.Models
﻿{
﻿    /// <summary>
﻿    /// Represents an error response from an API.
﻿    /// </summary>
﻿    public class Err
﻿    {
﻿        /// <summary>
﻿        /// Gets or sets the error message.
﻿        /// </summary>
﻿        [JsonPropertyName("message")]
﻿        public string Message { get; set; } = string.Empty;

﻿        /// <summary>
﻿        /// Gets or sets additional error details.
﻿        /// </summary>
﻿        [JsonPropertyName("extras")]
﻿        public IDictionary<string, string> Extras { get; set; } = new Dictionary<string, string>();

﻿        /// <summary>
﻿        /// Gets or sets the error code.
﻿        /// </summary>
﻿        [JsonPropertyName("code")]
﻿        public string? Code { get; set; }

﻿        /// <summary>
﻿        /// Gets or sets the timestamp when the error occurred.
﻿        /// </summary>
﻿        [JsonPropertyName("timestamp")]
﻿        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

﻿        /// <summary>
﻿        /// Creates a new error with the specified message.
﻿        /// </summary>
﻿        /// <param name="message">The error message.</param>
﻿        /// <returns>A new Err instance.</returns>
﻿        public static Err Create(string message)
﻿        {
﻿            return new Err { Message = message };
﻿        }

﻿        /// <summary>
﻿        /// Creates a new error with the specified message and code.
﻿        /// </summary>
﻿        /// <param name="message">The error message.</param>
﻿        /// <param name="code">The error code.</param>
﻿        /// <returns>A new Err instance.</returns>
﻿        public static Err Create(string message, string code)
﻿        {
﻿            return new Err { Message = message, Code = code };
﻿        }

﻿        /// <summary>
﻿        /// Creates a new error with the specified message and extra details.
﻿        /// </summary>
﻿        /// <param name="message">The error message.</param>
﻿        /// <param name="extras">Additional error details.</param>
﻿        /// <returns>A new Err instance.</returns>
﻿        public static Err Create(string message, IDictionary<string, string> extras)
﻿        {
﻿            return new Err 
﻿            { 
﻿                Message = message,
﻿                Extras = new Dictionary<string, string>(extras)
﻿            };
﻿        }

﻿        /// <summary>
﻿        /// Adds an extra detail to the error.
﻿        /// </summary>
﻿        /// <param name="key">The key for the extra detail.</param>
﻿        /// <param name="value">The value of the extra detail.</param>
﻿        /// <returns>The current Err instance for method chaining.</returns>
﻿        public Err AddExtra(string key, string value)
﻿        {
﻿            Extras[key] = value;
﻿            return this;
﻿        }

﻿        /// <summary>
﻿        /// Returns a string representation of the error.
﻿        /// </summary>
﻿        /// <returns>A string containing the error message and code if available.</returns>
﻿        public override string ToString()
﻿        {
﻿            var result = !string.IsNullOrEmpty(Code) 
﻿                ? $"[{Code}] {Message}" 
﻿                : Message;

﻿            if (Extras.Count > 0)
﻿            {
﻿                result += $" Details: {string.Join(", ", Extras.Select(x => $"{x.Key}={x.Value}"))}";
﻿            }

﻿            return result;
﻿        }
﻿    }
﻿}