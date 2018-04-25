using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace HttpTracer.TestApp.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public ICommand ButtonClickCommand
        {
            get { return new Command(async () => { await ButtonClick(); }); }
        }

        public MainPageViewModel(INavigationService navigationService) 
            : base (navigationService)
        {
            Title = "Main Page";
        }

        private async Task ButtonClick()
        {
            var client = new HttpClient(new HttpTracerHandler());
            client.DefaultRequestHeaders.Add("Authorization", "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6InFFZlhrMnVWMFhIU2k3czN2T2l1UDFjWnhWbjZyb2hkSjNtWnVzU0tDdFkifQ.eyJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vMDdlODdjZTktNDRhMS00MjE5LTljZjMtMWIyMmRmZDk0ZTM0L3YyLjAvIiwiZXhwIjoxNTIzODk5ODgwLCJuYmYiOjE1MjM4OTYyODAsImF1ZCI6IjRmNDk0OGEzLWY0NzUtNGE2Mi05MDYwLTlmZGFlYWM5NTg4ZCIsInN1YiI6IjZmZWE5OWRkLTlhMTAtNDkxNy05ZmUzLTA2NWI1MjViZjYxNCIsImVtYWlsIjoiZGNhdXNlckBidXJzdGluZ3NpbHZlci5jb20iLCJuYW1lIjoiRGFuaWVsIENhdXNlciIsImdpdmVuX25hbWUiOiJEYW5pZWwiLCJmYW1pbHlfbmFtZSI6IkNhdXNlciIsInNjcCI6ImRlZmF1bHQiLCJhenAiOiI0ZjQ5NDhhMy1mNDc1LTRhNjItOTA2MC05ZmRhZWFjOTU4OGQiLCJ2ZXIiOiIxLjAiLCJpYXQiOjE1MjM4OTYyODB9.HKgCxFkRVOYSUjZIZDeI4nEBZO28Zqhafr24PP5Rf4y5KqEbDrp48OH5M_8j1SFDgOsbFUk79kWWdOSUN9o5Ega8WfZurh0goqvRa9Z1IIAKfZp93zK9Fk5Sy53OG0naoObhND-dilVHYvOBGEq05cZDD03R8xRv4jFRw5bPLHkEI9WBfyEPPdKbwFliQmlPbufXUl3-u1XTHkfFoVW914y3-P-Ia0PAmDMujGhViJaZ61g15Lu9ueYETSQ454RgzpWbm6lOrlh6awkhsDGqZdOIUHGSOyd3PqXR72luZwW799fK_RCbKU3P1QjbLO1CNy7gLW5AuufuGzHNujWFXA");
            client.DefaultRequestHeaders.Add("client-version", "1.0.0");
            client.DefaultRequestHeaders.Add("custom-header", "Hi Mark");
            try
            {
                var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

   
}
