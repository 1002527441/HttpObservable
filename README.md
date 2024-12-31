# HttpObservable
Implement Observable httpClient in C#

![alt text](https://p4.itc.cn/images01/20220323/501fca03bb3c41e79326a80d22431b08.png)

## ✨Get Started    
- Any Blazor hosting model - Server, WebAssembly, or .NET MAUI   
- .NET 8 or later
    

## Installation
- Add the package HttpObservable. If you're using the command line, that's:
```csharp
    dotnet add package HttpObservable
```

## Usage
1. your services can inherit the HttpObservable, HttpObservable class has provide the following methods.
```csharp
    - public IAsyncObservable<TDto> PostAsJson<TDto, TPayload>(string url, TPayload data);
    - public IAsyncObservable<TDto> PostContent<TDto>(string url, HttpContent content);
    - public IAsyncObservable<TDto> PutAsJson<TDto, TPayload>(string url, TPayload data);
    - public IAsyncObservable<TDto> Delete<TDto>(string url);
    - public IAsyncObservable<TDto> Get<TDto>(string url);
```
- your main method in program, as we httpclient we used in HttpObservable, SampleService is our one of implementation of httpObservable.
```csharp
     public class Program
        {
            public static async Task Main(string[] args)
            {
                var builder = WebAssemblyHostBuilder.CreateDefault(args);
                builder.RootComponents.Add<App>("#app");
                builder.RootComponents.Add<HeadOutlet>("head::after");

                builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(ApiEnd.BaseUrl) });          
          
                builder.Services.AddScoped<SampleService>();

                await builder.Build().RunAsync();
            }
        }
```
- In our blazor page, you can use it like this.
```csharp
@page "/token"

@using HttpObservable.Models
@using HttpObservable.Samples.Services

@inject SampleService sampleService;

<h3>TokenPage</h3>

<p>@token</p>

<button class="btn btn-primary" @onclick="getToken">GetToken</button>

@code {

    private string token = string.Empty;
    private async void getToken(MouseEventArgs e)
    {
        var request = new { username = "jack.chen", password = "abcdsd23e23" };
        await sampleService!.SigninAsync(request.username, request.password)
            .SubscribeAsync(OnSuccess, OnError);

    }

    private void OnSuccess(ApiResponse<string> result)
    {
        if (!result.Succeeded)
        {
            Console.WriteLine(result.Error!.Message);
            return;
        }


        token = result.Data!;
        Console.Write(result);
        StateHasChanged();

    }

    private void OnError(Exception exception)
    {
        var error = exception.Message;
        Console.WriteLine(exception.ToString());
    }
}

```
## 🌈 Online Examples
- please refer to the project in our project. "HttpObservable Sample"
- we provide the ApiResponse<T>, ApiError, and PagedList<T>

**ApiResponse<string>** is the response you got from your webapi, just like this.  for sure, you can use your own customized response.

```csharp

    public class ApiResponse<TData>
    {
        public int Code { get; set; }
        public bool Succeeded { get; set; }
        public TData? Data { get; set; }
        public ApiError? Error { get; set; }
        public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
    }

    // here is my sample, I got the response from our webapi.
    {
      "code": 0,
      "succeeded": true,
      "data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOiJlMmVhZjAwYWNlM2M0OTE2OWQ1MmEzNzQ3YjdhNjM4ZiIsIlVzZXJuYW1lIjoiaGVucnkuemhhbmciLCJFbWFpbCI6IkhlbnJ5LlpoYW5nQHdlLW9ubGluZS5jb20iLCJEaXNwbGF5TmFtZSI6IlpoYW5nLCBIZW5yeSAoUVMpIiwiVGl0bGUiOiJUZWFtIGxlYWRlciIsIkRlcGFydG1lbnQiOiJRdWFsaXR5IERlc2lnbiBDZW50ZXIgQXNpYSBBcHBsaWNhdGlvbiBEZXNpZ24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOlsiVXNlciIsIlB1cmNoYXNlciIsIlByb2N1cmVtZW50IE1hbmFnZXIiLCJGaW5hbmNlIE1hbmFnZXIiLCJGaW5hbmNlIiwiQWRtaW4iLCJBcHByb3ZlciIsIkRldmVsb3BlciJdLCJleHAiOjE3MzU2NTgzMTIsImlzcyI6IldFIiwiYXVkIjoiV0UifQ.IdSesRJLRPkt9Pl8yyJrHer1EjwtoUikT1dOsBO179g",
      "error": null,
      "timestamp": 638712119125196300
    }
```
## Finally
-if you meet any question, please feel free contact me.
