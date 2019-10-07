using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using HttpTracer.Logger;
using MvvmHelpers;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace HttpTracer.TestApp.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private const string Domain = "reqres.in";
        private const HttpMessageParts DefaultHttpTracerVerbosity =
            HttpMessageParts.RequestAll | HttpMessageParts.ResponseHeaders;

        public ICommand LogToScreenCommand { get; }

        public ICommand LogToConsoleCommand { get; }
        
        public ObservableRangeCollection<string> LogEntries { get; } = new ObservableRangeCollection<string>();
        
        private string _logEntriesString;
        public string LogEntriesString
        {
            get => _logEntriesString;
            set => SetProperty(ref _logEntriesString, value);
        }
        
        private FormattedString _logEntriesSpans = new FormattedString();
        public FormattedString LogEntriesSpans
        {
            get => _logEntriesSpans;
            set => SetProperty(ref _logEntriesSpans, value);
        }

        public ObservableRangeCollection<User> UserList { get; } = new ObservableRangeCollection<User>();

        public MainPageViewModel(INavigationService navigationService)
            : base (navigationService)
        {
            Title = "Main Page";
            LogToConsoleCommand = new Command(async () => { await LogToConsole(); });
            LogToScreenCommand = new Command(async () => { await LogToScreen(); });
        }

        private async Task LogToConsole()
        {
            await GetData(new DebugLogger());
        }

        private async Task PostData(ILogger logger)
        {
            var client = GetHttpClient(logger);
            try
            {
                var result = await client.GetAsync($"https://{Domain}/api/users");
                var json = await result.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);
                var users = jObject["data"].ToObject<List<User>>();
                UserList.AddRange(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static HttpClient GetHttpClient(ILogger logger)
        {
            HttpClient client = default;
            try
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.UseCookies = true;
                httpClientHandler.CookieContainer.Add(new Cookie("TestCookie1", "One", "", Domain));
                httpClientHandler.CookieContainer.Add(new Cookie("TestCookie2", "Two", "", Domain));
            
                client = new HttpClient(new HttpTracerHandler(httpClientHandler, logger, DefaultHttpTracerVerbosity));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer ThisIsProbablyNotAValidJwt");
                client.DefaultRequestHeaders.Add("client-version", "1.0.0");
                client.DefaultRequestHeaders.Add("custom-header", "Probably not the matrix");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return client;
        }

        private static async Task GetData(ILogger logger)
        {
            var client = GetHttpClient(logger);
            try
            {
                //var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");
                var content = new StringContent(@"{""name"": ""morpheus"", ""job"": ""leader""}");
                await client.PostAsync($"https://{Domain}/api/users", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task LogToScreen()
        {
            var memoryLogger = new MemoryLogger();
            await PostData(memoryLogger);
            LogEntries.AddRange(memoryLogger.LogEntries.SelectMany(x => x.Split(Environment.NewLine.ToCharArray())));
            LogEntriesString += string.Join(Environment.NewLine, LogEntries);
            LogEntriesSpans = new FormattedString();
            foreach (var logEntry in LogEntries)
            {
                LogEntriesSpans.Spans.Add(new Span { Text = logEntry });
                LogEntriesSpans.Spans.Add(new Span { Text = Environment.NewLine });
            }
        }

        public class MemoryLogger : ILogger
        {
            private readonly List<string> _logEntries = new List<string>();

            public List<string> LogEntries => _logEntries;

            public void Log(string message)
            {
                _logEntries.Add(message);
            }
        }
    }
}
