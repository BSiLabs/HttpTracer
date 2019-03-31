using System.Net.Http;
using System.Threading.Tasks;
using HttpTracer.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpTracer.Tests
{
    [TestClass]
    public class HttpTracerTests
    {
        [TestMethod]
        public async Task ShouldLogWithoutBuilder()
        {
            var logger = new FakeLogger();
            var handler = new FakeHttpTraceHandler(logger);

            var client = new HttpClient(handler);
            await client.GetAsync("https://uinames.com/api?ext&amount=25");

            Assert.AreEqual(7, logger.LogHistory.Count);
        }

        [TestMethod]
        public async Task ShouldLogWithHandlerHierarchyWithoutBuilder()
        {
            var logger = new FakeLogger();
            var child = new SillyHandler { InnerHandler = new FakeHandler { InnerHandler = new FakeHttpTraceHandler(logger) } };


            var client = new HttpClient(child);
            await client.GetAsync("https://uinames.com/api?ext&amount=25");

            Assert.AreEqual(7, logger.LogHistory.Count);
        }

        [TestMethod]
        public async Task ShouldLogRequestAllAndResponseAll()
        {
            var verbosity = HttpMessageParts.All;
            var logger = await ExecuteFakeRequest(verbosity);
            
            Assert.AreEqual(7, logger.LogHistory.Count);
        }

        [TestMethod]
        public async Task ShouldLogRequestAllResponseHeaders()
        {
            var verbosity = HttpMessageParts.RequestAll | HttpMessageParts.ResponseHeaders;
            var logger = await ExecuteFakeRequest(verbosity);
            
            Assert.AreEqual(6, logger.LogHistory.Count);
        }

        [TestMethod]
        public async Task ShouldLogRequestAll()
        {
            var verbosity = HttpMessageParts.RequestAll;
            var logger = await ExecuteFakeRequest(verbosity);
            
            Assert.AreEqual(3, logger.LogHistory.Count);
        }

        [TestMethod]
        public async Task ShouldLogRequestHeaders()
        {
            var verbosity = HttpMessageParts.RequestHeaders;
            var logger = await ExecuteFakeRequest(verbosity);
            
            Assert.AreEqual(2, logger.LogHistory.Count);
        }

        [TestMethod]
        public async Task ShouldLogNothing()
        {
            var verbosity = HttpMessageParts.None;
            var logger = await ExecuteFakeRequest(verbosity);
            
            Assert.AreEqual(0, logger.LogHistory.Count);
        }

        private static async Task<FakeLogger> ExecuteFakeRequest(HttpMessageParts? verbosity = null)
        {
            var logger = new FakeLogger();
            var builder = new HttpHandlerBuilder(new FakeHttpTraceHandler(logger));
            builder.AddHandler(new FakeHandler())
                .AddHandler(new SillyHandler())
                .AddHandler(new FakeHandler());
            
            if (verbosity != null)
                builder.SetHttpTracerVerbosity(verbosity.Value);

            var client = new HttpClient(builder.Build());
            await client.GetAsync("https://uinames.com/api?ext&amount=25");
            return logger;
        }
    }
}
