using HttpObservable.Models;
using HttpObservable.Samples.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HttpObservable_Samples.Pages
{
    public partial class Home
    {
        [Inject] AuthService? authService {  get; set; }
        private string token = string.Empty;
        private async void getToken(MouseEventArgs e)
        {
            var request = new { username = "henry.zhang", password = "Mug27hz10008" };
           await  authService!.SigninAsync(request.username, request.password)
                .SubscribeAsync(OnSuccess, OnError);
            
        }

        private async void getItem(MouseEventArgs e)
        {
            
            await authService!.GetItems()
                 .SubscribeAsync(OnSuccess, OnError);

        }

        private void OnError(Exception exception)
        {
            var error = exception.Message;
            Console.WriteLine(exception.ToString());
        }

        private void OnSuccess(ApiResponse<PagedList<ItemDto>> response)
        {
             Console.WriteLine(response.Data);
            StateHasChanged();
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


    }
}
