using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpTracer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpTracerTests
{
    public class MyHandler1 : DelegatingHandler
    {
        public MyHandler1()
        {
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);

            request.Headers.Add("SILLY-HEADER", "SILLY VALUE");

            Debug.WriteLine("HI I'M MyHandler1");

            await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

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

            await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            return new HttpResponseMessage();
        }
    }
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public async Task BuildingMandlers()
        {
            var root = new HttpTracerHandler { InnerHandler = new MyHandler3 { InnerHandler = new MyHandler1() } };

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

        [TestMethod]
        public async Task UsinBuilderClass()
        {
            var builder = new HttpHandlerBuilder();
            builder.AddHttpHandlers(new MyHandler3())
                .AddHttpHandlers(new MyHandler1())
                .AddHttpHandlers(new MyHandler3());

            var client = new HttpClient(builder.Build());
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
