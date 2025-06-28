﻿using System;
﻿using System.Text.Json;
﻿using System.Text;
﻿using System.Net.Http.Headers;

﻿namespace HttpObservable
﻿{
﻿    /// <summary>
﻿    /// Helper class for creating HTTP requests and handling content.
﻿    /// </summary>
﻿    public static class HttpRequestHelper
﻿    {
﻿        private static readonly JsonSerializerOptions DefaultJsonOptions = new()
﻿        {
﻿            PropertyNameCaseInsensitive = true,
﻿            WriteIndented = false
﻿        };

﻿        /// <summary>
﻿        /// Creates an HTTP request message with the specified method, URL and optional content.
﻿        /// </summary>
﻿        /// <param name="method">The HTTP method for the request.</param>
﻿        /// <param name="url">The URL for the request.</param>
﻿        /// <param name="content">Optional content to include in the request.</param>
﻿        /// <param name="jsonOptions">Optional JSON serialization options.</param>
﻿        /// <returns>A configured HttpRequestMessage.</returns>
﻿        /// <exception cref="ArgumentNullException">Thrown when method or url is null.</exception>
﻿        /// <exception cref="ArgumentException">Thrown when url is empty or whitespace.</exception>
﻿        public static HttpRequestMessage CreateHttpRequest(
﻿            HttpMethod method,
﻿            string url,
﻿            object? content = null,
﻿            JsonSerializerOptions? jsonOptions = null)
﻿        {
﻿            if (method == null) throw new ArgumentNullException(nameof(method));
﻿            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));

﻿            var request = new HttpRequestMessage(method, url);

﻿            if (content == null) return request;

﻿            request.Content = content switch
﻿            {
﻿                HttpContent httpContent => httpContent,
﻿                string stringContent => new StringContent(stringContent, Encoding.UTF8, "text/plain"),
﻿                Stream streamContent => new StreamContent(streamContent),
﻿                byte[] byteContent => new ByteArrayContent(byteContent),
﻿                _ => new StringContent(
﻿                    JsonSerializer.Serialize(content, jsonOptions ?? DefaultJsonOptions),
﻿                    Encoding.UTF8,
﻿                    "application/json")
﻿            };

﻿            return request;
﻿        }

﻿        /// <summary>
﻿        /// Creates content for uploading a single file.
﻿        /// </summary>
﻿        /// <param name="fileStream">The stream containing the file data.</param>
﻿        /// <param name="fileName">The name of the file.</param>
﻿        /// <param name="paramName">The form parameter name for the file.</param>
﻿        /// <param name="contentType">Optional content type of the file.</param>
﻿        /// <returns>HttpContent configured for file upload.</returns>
﻿        /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
﻿        public static HttpContent CreateUploadFileContent(
﻿            Stream fileStream,
﻿            string fileName,
﻿            string paramName = "file",
﻿            string? contentType = null)
﻿        {
﻿            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
﻿            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be empty.", nameof(fileName));
﻿            if (string.IsNullOrWhiteSpace(paramName)) throw new ArgumentException("Parameter name cannot be empty.", nameof(paramName));

﻿            var streamContent = new StreamContent(fileStream);
﻿            if (!string.IsNullOrWhiteSpace(contentType))
﻿            {
﻿                streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
﻿            }

﻿            var multipartContent = new MultipartFormDataContent();
﻿            multipartContent.Add(streamContent, paramName, fileName);
﻿            return multipartContent;
﻿        }

﻿        /// <summary>
﻿        /// Creates content for uploading multiple files.
﻿        /// </summary>
﻿        /// <param name="fileStreams">Array of streams containing file data.</param>
﻿        /// <param name="fileNames">Array of file names.</param>
﻿        /// <param name="paramName">The form parameter name for the files.</param>
﻿        /// <param name="contentTypes">Optional array of content types for each file.</param>
﻿        /// <returns>HttpContent configured for multiple file upload.</returns>
﻿        /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
﻿        /// <exception cref="ArgumentException">Thrown when arrays have different lengths.</exception>
﻿        public static HttpContent CreateUploadFilesContent(
﻿            Stream[] fileStreams,
﻿            string[] fileNames,
﻿            string paramName = "files",
﻿            string[]? contentTypes = null)
﻿        {
﻿            if (fileStreams == null) throw new ArgumentNullException(nameof(fileStreams));
﻿            if (fileNames == null) throw new ArgumentNullException(nameof(fileNames));
﻿            if (fileStreams.Length != fileNames.Length)
﻿                throw new ArgumentException("Number of file streams must match number of file names.");
﻿            if (contentTypes != null && contentTypes.Length != fileStreams.Length)
﻿                throw new ArgumentException("Number of content types must match number of files.");

﻿            var multipartContent = new MultipartFormDataContent();

﻿            for (int i = 0; i < fileStreams.Length; i++)
﻿            {
﻿                if (fileStreams[i] == null)
﻿                    throw new ArgumentException($"File stream at index {i} is null.");
﻿                if (string.IsNullOrWhiteSpace(fileNames[i]))
﻿                    throw new ArgumentException($"File name at index {i} is empty or null.");

﻿                var streamContent = new StreamContent(fileStreams[i]);
﻿                if (contentTypes != null && !string.IsNullOrWhiteSpace(contentTypes[i]))
﻿                {
﻿                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentTypes[i]);
﻿                }

﻿                multipartContent.Add(streamContent, paramName, fileNames[i]);
﻿            }

﻿            return multipartContent;
﻿        }

﻿        /// <summary>
﻿        /// Creates form URL encoded content from a dictionary of values.
﻿        /// </summary>
﻿        /// <param name="formData">Dictionary containing form data.</param>
﻿        /// <returns>FormUrlEncodedContent configured with the provided form data.</returns>
﻿        /// <exception cref="ArgumentNullException">Thrown when formData is null.</exception>
﻿        public static FormUrlEncodedContent CreateFormContent(IDictionary<string, string> formData)
﻿        {
﻿            if (formData == null) throw new ArgumentNullException(nameof(formData));
﻿            return new FormUrlEncodedContent(formData);
﻿        }
﻿    }
﻿}