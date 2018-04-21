using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpTracer.Tests
{
    [TestClass]
    public class LoggerTests
    {
        public class TestLogger : ILogger
        {
            public List<string> LogHistory = new List<string>();
            public void Log(string message)
            {
                LogHistory.Add(message);
            }
        }

        [TestMethod]
        public async Task ShouldLogWithoutBuilder()
        {
            var logger = new TestLogger();
            var root = new HttpTracerHandler(logger) { InnerHandler = new MyHandler3 { InnerHandler = new MyHandler1() } };

            var child = new MyHandler1 { InnerHandler = new MyHandler3 { InnerHandler = new HttpTracerHandler() } };


            var client = new HttpClient(child);
            var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");

            // wait for the logging to complete (fire and forget)
            await Task.Delay(2000);

            Assert.AreEqual(2, logger.LogHistory.Count);
        }

        [TestMethod]
        public async Task ShouldLogWithBuilder()
        {
            var logger = new TestLogger();
            var builder = new HttpHandlerBuilder(logger);
            builder.AddHandler(new MyHandler3())
                .AddHandler(new MyHandler1())
                .AddHandler(new MyHandler3());

            var client = new HttpClient(builder.Build());
            var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");

            // wait for the logging to complete (fire and forget)
            await Task.Delay(2000);

            Assert.AreEqual(2, logger.LogHistory.Count);
        }
    }
}
