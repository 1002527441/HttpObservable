using System.Text.Json;
using System.Text;

namespace HttpObservable
{
    public static class HttpRequestHelper
    {
        public static HttpRequestMessage CreateHttpRequest(HttpMethod method, string url, object? content = null)
        {
            var request = new HttpRequestMessage(method, url);

            if (content == null) return request;

            switch (content)
            {
                case HttpContent httpContent:
                    request.Content = httpContent;
                    break;
                default:
                    var json = JsonSerializer.Serialize(content);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    break;
            }


            return request;
        }

        
        //upload a single file
        public static HttpContent CreateUploadFileContent(Stream fileStream, string fileName, string paramName="file")
        {
            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(new StreamContent(fileStream), paramName, fileName);
            return multipartContent;
        }



        //upload multiple files
        public static HttpContent CreateUploadFilesContent(Stream[] fileStreams, string[] fileNames, string paramName = "files")
        {
            var multipartContent = new MultipartFormDataContent();

            for (int i = 0; i < fileStreams.Length; i++)
            {
                multipartContent.Add(new StreamContent(fileStreams[i]), paramName, fileNames[i]);
            }

            return multipartContent;
        }


    }
}
