using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpTracer.Tests
{
    [TestClass]
    class LoggerTests
    {
        [TestMethod]
        public async Task BuildingMandlers()
        {
            var root = new HttpTracerHandler { InnerHandler = new MyHandler3 { InnerHandler = new MyHandler1() } };

            var child = new MyHandler1 { InnerHandler = new MyHandler3 { InnerHandler = new HttpTracerHandler() } };


            var client = new HttpClient(child);
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
        public async Task UsingBuilderClass()
        {
            var builder = new HttpHandlerBuilder();
            builder.AddHandler(new MyHandler3())
                .AddHandler(new MyHandler1())
                .AddHandler(new MyHandler3());

            var client = new HttpClient(builder.Build());
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
