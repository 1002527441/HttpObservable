using HttpObservable.Models;
using HttpObservable_Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpObservable.Samples.Services
{
    public class AuthService:HttpObservable
    {
        private readonly ILogger<AuthService> _logger;

        public AuthService(ILogger<AuthService> logger, HttpClient http):base(http)
        {
            _logger = logger;
        }

        public IAsyncObservable<string> SigninAsync(string username, string password)
        {
            var url = "/api/v1/auth/signin"; 
            _logger.LogInformation(_http.BaseAddress + url);
            var request = new SigninRequest(username, password);
            return base.PostAsJson<string, SigninRequest>(url, request);
        }

        public IAsyncObservable<PagedList<ItemDto>> GetItems()
        {
            var url = "/api/v1/items?page=1&pageSize=20";

            return base.Get<PagedList<ItemDto>>(url);
            
        }
    }
}
