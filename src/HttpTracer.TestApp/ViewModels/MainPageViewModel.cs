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
using HttpTracer.Logger;
using Xamarin.Forms;

namespace HttpTracer.TestApp.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private const HttpMessageParts DefaultHttpTracerVerbosity =
            HttpMessageParts.RequestAll | HttpMessageParts.ResponseHeaders;
        
        public ICommand ButtonClickCommand
        {
            get { return new Command(async () => { await ButtonClick(); }); }
        }

        public ICommand TraceFileCommand
        {
            get { return new Command(async () => { await TraceFileClick(); }); }
        }

        public MainPageViewModel(INavigationService navigationService)
            : base (navigationService)
        {
            Title = "Main Page";
        }

        private async Task ButtonClick()
        {
            var client = new HttpClient(new HttpTracerHandler(null, new Logger.DebugLogger(), DefaultHttpTracerVerbosity));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer ThisIsProbablyNotAValidJwt");
            client.DefaultRequestHeaders.Add("client-version", "1.0.0");
            client.DefaultRequestHeaders.Add("custom-header", "Hi Mark");
            try
            {
                //var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");
                var content = new StringContent(@"{""name"": ""morpheus"", ""job"": ""leader""}");
                var result = await client.PostAsync("https://reqres.in/api/users", content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task TraceFileClick()
        {
            var client = new HttpClient(new HttpTracerHandler(null, new MyLogger(), DefaultHttpTracerVerbosity));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer ThisIsProbablyNotAValidJwt");
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

        public class MyLogger : ILogger
        {
            public void Log(string message)
            {
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@"C:\Users\danielcauser\HttpLog.txt", true))
                {
                    file.WriteLine(message);
                }
            }
        }
    }
}
