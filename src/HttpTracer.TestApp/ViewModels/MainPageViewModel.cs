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


            var root = new HttpTracerHandler{InnerHandler = new MyHandler3{ InnerHandler = new MyHandler1()} };

           var child = new MyHandler1 { InnerHandler = new MyHandler3 { InnerHandler = new HttpTracerHandler() } };


            var client = new HttpClient(child);
            //var client = new HttpClient(root);
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
