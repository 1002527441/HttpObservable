using HttpObservable.Models;
using HttpObservable_Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpObservable.Samples.Services
{
    public class SampleService:HttpObservable
    {
        private readonly ILogger<SampleService> _logger;

        public SampleService(ILogger<SampleService> logger, HttpClient http):base(http)
        {
            _logger = logger;
        }

        public IAsyncObservable<ApiResponse<string>> SigninAsync(string username, string password)
        {
            var url = "/api/v1/auth/signin"; 
            _logger.LogInformation(_http.BaseAddress + url);
            var request = new SigninRequest(username, password);
            return base.PostAsJson<ApiResponse<string>, SigninRequest>(url, request);
        }

        public IAsyncObservable<ApiResponse<PagedList<ItemDto>>> GetItems()
        {
            var url = "/api/v1/items?page=1&pageSize=20";

            return base.Get<ApiResponse<PagedList<ItemDto>>>(url);
            
        }
    }
}
