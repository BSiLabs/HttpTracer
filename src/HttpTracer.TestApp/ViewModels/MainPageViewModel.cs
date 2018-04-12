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

            HttpHandlerBuilder builder = new HttpHandlerBuilder();

            builder.AddDelegatingHandler(new MyHandler1())
                .AddDelegatingHandler(new MyHandler2())
                .AddDelegatingHandler(new MyHandler3());

            //var pipeline = new MyHandler1
            //{
            //    InnerHandler = new HttpTracerHandler()
            //};

            var client = new HttpClient(builder.Build());
            //var client = new HttpClient(new HttpTracerHandler());
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

    public class MyHandler1 : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);

            request.Headers.Add("SILLY-HEADER", "SILLY VALUE");

            Debug.WriteLine("HI I'M MyHandler1");

            return new HttpResponseMessage();

        }
    }

    public class MyHandler2 : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);

            Debug.WriteLine("HI I'M MyHandler2");

            return new HttpResponseMessage();

        }
    }
    public class MyHandler3 : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);
            request.Headers.Add("SILLY-HEADER-3", "SILLY VALUE 3");
           
            return new HttpResponseMessage();
        }
    }
}
