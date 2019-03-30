using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
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

        public class FakeHttpTraceHandler : HttpTracerHandler
        {
            public FakeHttpTraceHandler(ILogger logger) : base(null, logger) {}

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                try
                {
                    await LogHttpRequest(request);
                    var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("Response Content")};
                    await LogHttpResponse(response, 0);
                    return response;
                }
                catch (Exception ex)
                {
                    LogHttpException(request, ex);
                    throw;
                }
            }
        }

        public class FakeExceptionHttpTraceHandler : HttpTracerHandler
        {
            public FakeHttpTraceHandler(ILogger logger) : base(logger) {}

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                try
                {
                    await LogHttpRequest(request);
                    throw new ApplicationException("It done broke.");
                }
                catch (Exception ex)
                {
                    LogHttpException(request, ex);
                    throw;
                }
            }
        }

        [TestMethod]
        public async Task ShouldLogWithoutBuilder()
        {
            var logger = new TestLogger();
            var handler = new FakeHttpTraceHandler(logger);

            var client = new HttpClient(handler);
            var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");

            Assert.AreEqual(2, logger.LogHistory.Count);
        }

        [TestMethod]
        public async Task ShouldLogWithHandlerHierarchyWithoutBuilder()
        {
            var logger = new TestLogger();
            var child = new MyHandler1 { InnerHandler = new MyHandler3 { InnerHandler = new FakeHttpTraceHandler(logger) } };


            var client = new HttpClient(child);
            var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");

            Assert.AreEqual(2, logger.LogHistory.Count);
        }

        [TestMethod]
        public async Task ShouldLogWithBuilder()
        {
            var logger = new TestLogger();
            var builder = new HttpHandlerBuilder(new FakeHttpTraceHandler(logger));
            builder.AddHandler(new MyHandler3())
                .AddHandler(new MyHandler1())
                .AddHandler(new MyHandler3())
                .SetHttpTracerVerbosity(LogLevel.Trace);

            var client = new HttpClient(builder.Build());
            var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");

            Assert.AreEqual(2, logger.LogHistory.Count);
        }

        [TestMethod]
        public async Task ShouldLogResponseOnlyWithBuilder()
        {
            var logger = new TestLogger();
            var builder = new HttpHandlerBuilder(new FakeHttpTraceHandler(logger));
            builder.AddHandler(new MyHandler3())
                .AddHandler(new MyHandler1())
                .AddHandler(new MyHandler3())
                .SetHttpTracerVerbosity(LogLevel.Trace);

            var client = new HttpClient(builder.Build());
            var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");

            Assert.AreEqual(2, logger.LogHistory.Count);
        }
        
        // Trace - full requests and responses for all http requests
        [TestMethod]
        public async Task ShouldLogFullRequestAndResponseForTrace()
        {
            var logger = new TestLogger();
            var builder = new HttpHandlerBuilder(new FakeHttpTraceHandler(logger));
            builder.AddHandler(new MyHandler3())
                .AddHandler(new MyHandler1())
                .AddHandler(new MyHandler3())
                .SetHttpTracerVerbosity(LogLevel.Trace);

            var client = new HttpClient(builder.Build());
            var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");

            Assert.Fail();
            Assert.AreEqual(2, logger.LogHistory.Count);
        }
        
        // Debug - full requests, responses without body
        [TestMethod]
        public async Task ShouldLogFullRequestResponseWithoutBodyForDebug()
        {
            var logger = new TestLogger();
            var builder = new HttpHandlerBuilder(new FakeHttpTraceHandler(logger));
            builder.AddHandler(new MyHandler3())
                .AddHandler(new MyHandler1())
                .AddHandler(new MyHandler3())
                .SetHttpTracerVerbosity(LogLevel.Trace);

            var client = new HttpClient(builder.Build());
            var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");

            Assert.Fail();
            Assert.AreEqual(2, logger.LogHistory.Count);
        }
        
        // Information - full request and response for errors, request and response method/url/header for all requests
        [TestMethod]
        public async Task ShouldLogSimpleRequestAndResponseForInformation()
        {
            var logger = new TestLogger();
            var builder = new HttpHandlerBuilder(new FakeHttpTraceHandler(logger));
            builder.AddHandler(new MyHandler3())
                .AddHandler(new MyHandler1())
                .AddHandler(new MyHandler3())
                .SetHttpTracerVerbosity(LogLevel.Trace);

            var client = new HttpClient(builder.Build());
            var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");

            Assert.Fail();
            Assert.AreEqual(2, logger.LogHistory.Count);
        }
        
        // Critical - full request and response for errors
        [TestMethod]
        public async Task ShouldLogFullRequestAndResponseForUnsuccessfulResponseForCritical()
        {
            var logger = new TestLogger();
            var builder = new HttpHandlerBuilder(new FakeHttpTraceHandler(logger));
            builder.AddHandler(new MyHandler3())
                .AddHandler(new MyHandler1())
                .AddHandler(new MyHandler3())
                .SetHttpTracerVerbosity(LogLevel.Trace);

            var client = new HttpClient(builder.Build());
            var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");

            Assert.Fail();
            Assert.AreEqual(2, logger.LogHistory.Count);
        }
        
        // Error - request method/url/header and full response for errors only
        [TestMethod]
        public async Task ShouldLogSimpleRequestAndResponseForUnsuccessfulResponseForError()
        {
            var logger = new TestLogger();
            var builder = new HttpHandlerBuilder(new FakeHttpTraceHandler(logger));
            builder.AddHandler(new MyHandler3())
                .AddHandler(new MyHandler1())
                .AddHandler(new MyHandler3())
                .SetHttpTracerVerbosity(LogLevel.Trace);

            var client = new HttpClient(builder.Build());
            var result = await client.GetAsync("https://uinames.com/api?ext&amount=25");

            Assert.Fail();
            Assert.AreEqual(2, logger.LogHistory.Count);
        }
    }
}
