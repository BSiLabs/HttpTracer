using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using HttpTracer.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpTracer.Tests
{
    [TestClass]
    public class HttpTracerTests
    {
        private const string TestUri = "https://uinames.com/api?ext&amount=25";
        public const string CookieName = "TEST-COOKIE";
        public const string CookieValue = "TEST COOKIE";

        [TestMethod]
        public async Task ShouldLogWithoutBuilder()
        {
            var logger = new FakeLogger();
            var handler = new FakeHttpTraceHandler(null, logger);

            var client = new HttpClient(handler);
            await client.GetAsync(TestUri);

            // assert log count
            logger.LogHistory.Count.Should().Be(2);

            // assert request
            logger.LogHistory[0].Should().Contain(TestUri);
            logger.LogHistory[0].Should().Contain("HTTP REQUEST: [GET]");

            // assert response
            logger.LogHistory[1].Should().Contain("StatusCode: 200");
            logger.LogHistory[1].Should().Contain("ReasonPhrase: 'OK'");
            logger.LogHistory[1].Should().Contain("Content-Type: application/json; charset=utf-8");
            logger.LogHistory[1].Should().Contain(FakeHttpTraceHandler.FakeResponseContent);
        }

        [TestMethod]
        public async Task ShouldLogWithHandlerHierarchyWithoutBuilder()
        {
            var logger = new FakeLogger();
            var child = new SillyHandler {InnerHandler = new FakeHandler {InnerHandler = new FakeHttpTraceHandler(null, logger)}};

            var client = new HttpClient(child);
            await client.GetAsync(TestUri);

            // assert log count
            logger.LogHistory.Count.Should().Be(2);

            // assert request
            logger.LogHistory[0].Should().Contain(TestUri);
            logger.LogHistory[0].Should().Contain("HTTP REQUEST: [GET]");
            logger.LogHistory[0].Should().Contain(SillyHandler.SillyHeader);
            logger.LogHistory[0].Should().Contain(FakeHandler.FakeHeader);

            // assert response
            logger.LogHistory[1].Should().Contain("StatusCode: 200");
            logger.LogHistory[1].Should().Contain("ReasonPhrase: 'OK'");
            logger.LogHistory[1].Should().Contain("Content-Type: application/json; charset=utf-8");
            logger.LogHistory[1].Should().Contain(FakeHttpTraceHandler.FakeResponseContent);
        }

        [TestMethod]
        public async Task ShouldLogRequestAllAndResponseAll()
        {
            var verbosity = HttpMessageParts.All;
            var logger = await ExecuteFakeRequest(verbosity);

            // assert log count
            logger.LogHistory.Count.Should().Be(2);

            // assert request
            logger.LogHistory[0].Should().Contain(TestUri);
            logger.LogHistory[0].Should().Contain("HTTP REQUEST: [POST]");
            logger.LogHistory[0].Should().Contain(SillyHandler.SillyHeader);
            logger.LogHistory[0].Should().Contain(FakeHandler.FakeHeader);

            // assert response
            logger.LogHistory[1].Should().Contain("StatusCode: 200");
            logger.LogHistory[1].Should().Contain("ReasonPhrase: 'OK'");
            logger.LogHistory[1].Should().Contain("Content-Type: application/json; charset=utf-8");
            logger.LogHistory[1].Should().Contain("Duration: 00:0250000");
            logger.LogHistory[1].Should().Contain(FakeHttpTraceHandler.FakeResponseContent);
        }

        [TestMethod]
        public async Task ShouldLogRequestBodyAndResponseAll()
        {
            var verbosity = HttpMessageParts.RequestBody | HttpMessageParts.ResponseAll;
            var logger = await ExecuteFakeRequest(verbosity);

            // assert log count
            logger.LogHistory.Count.Should().Be(2);

            // assert request
            logger.LogHistory[0].Should().Contain(TestUri);
            logger.LogHistory[0].Should().Contain("HTTP REQUEST: [POST]");
            logger.LogHistory[0].Should().NotContain(SillyHandler.SillyHeader);
            logger.LogHistory[0].Should().NotContain(FakeHandler.FakeHeader);

            // assert response
            logger.LogHistory[1].Should().Contain("StatusCode: 200");
            logger.LogHistory[1].Should().Contain("ReasonPhrase: 'OK'");
            logger.LogHistory[1].Should().Contain("Content-Type: application/json; charset=utf-8");
            logger.LogHistory[1].Should().Contain(FakeHttpTraceHandler.FakeResponseContent);
        }

        [TestMethod]
        public async Task ShouldLogRequestAllResponseHeaders()
        {
            var verbosity = HttpMessageParts.RequestAll | HttpMessageParts.ResponseHeaders;
            var logger = await ExecuteFakeRequest(verbosity);

            // assert log count
            logger.LogHistory.Count.Should().Be(2);

            // assert request
            logger.LogHistory[0].Should().Contain(TestUri);
            logger.LogHistory[0].Should().Contain("HTTP REQUEST: [POST]");
            logger.LogHistory[0].Should().Contain(SillyHandler.SillyHeader);
            logger.LogHistory[0].Should().Contain(FakeHandler.FakeHeader);

            // assert response
            logger.LogHistory[1].Should().Contain("StatusCode: 200");
            logger.LogHistory[1].Should().Contain("ReasonPhrase: 'OK'");
            logger.LogHistory[1].Should().Contain("Content-Type: application/json; charset=utf-8");
            logger.LogHistory[1].Should().NotContain(FakeHttpTraceHandler.FakeResponseContent);
        }

        [TestMethod]
        public async Task ShouldLogRequestAll()
        {
            var verbosity = HttpMessageParts.RequestAll;
            var logger = await ExecuteFakeRequest(verbosity);

            // assert log count
            logger.LogHistory.Count.Should().Be(1);

            // assert request
            logger.LogHistory[0].Should().Contain(TestUri);
            logger.LogHistory[0].Should().Contain("HTTP REQUEST: [POST]");
            logger.LogHistory[0].Should().Contain(SillyHandler.SillyHeader);
            logger.LogHistory[0].Should().Contain(FakeHandler.FakeHeader);
        }

        [TestMethod]
        public async Task ShouldLogRequestHeaders()
        {
            var verbosity = HttpMessageParts.RequestHeaders;
            var logger = await ExecuteFakeRequest(verbosity);

            // assert log count
            logger.LogHistory.Count.Should().Be(1);

            // assert request
            logger.LogHistory[0].Should().Contain(TestUri);
            logger.LogHistory[0].Should().Contain("HTTP REQUEST: [POST]");
            logger.LogHistory[0].Should().Contain(SillyHandler.SillyHeader);
            logger.LogHistory[0].Should().Contain(FakeHandler.FakeHeader);
        }

        [TestMethod]
        public async Task ShouldLogRequestCookies()
        {
            var verbosity = HttpMessageParts.RequestAll;
            var logger = await ExecuteFakeRequest(verbosity);

            // assert log count
            logger.LogHistory.Count.Should().Be(1);

            // assert request
            logger.LogHistory[0].Should().Contain(TestUri);
            logger.LogHistory[0].Should().Contain("HTTP REQUEST: [POST]");
            logger.LogHistory[0].Should().Contain(SillyHandler.SillyHeader);
            logger.LogHistory[0].Should().Contain(FakeHandler.FakeHeader);
            logger.LogHistory[0].Should().Contain(CookieName);
        }

        [TestMethod]
        public async Task ShouldLogNothing()
        {
            var verbosity = HttpMessageParts.None;
            var logger = await ExecuteFakeRequest(verbosity);

            logger.LogHistory.Count.Should().Be(0);
        }


        [TestMethod]
        public async Task ShouldChangeTheDurationFormat()
        {
            HttpTracerHandler.DefaultDurationFormat = "{0:ffffff}ms";
            var verbosity = HttpMessageParts.All;
            var logger = await ExecuteFakeRequest(verbosity);

            // assert log count
            logger.LogHistory.Count.Should().Be(2);

            // assert request
            logger.LogHistory[0].Should().Contain(TestUri);
            logger.LogHistory[0].Should().Contain("HTTP REQUEST: [POST]");
            logger.LogHistory[0].Should().Contain(SillyHandler.SillyHeader);
            logger.LogHistory[0].Should().Contain(FakeHandler.FakeHeader);

            // assert response
            logger.LogHistory[1].Should().Contain("StatusCode: 200");
            logger.LogHistory[1].Should().Contain("ReasonPhrase: 'OK'");
            logger.LogHistory[1].Should().Contain("Content-Type: application/json; charset=utf-8");
            logger.LogHistory[1].Should().Contain("025000ms");
            logger.LogHistory[1].Should().Contain(FakeHttpTraceHandler.FakeResponseContent);
        }

        [TestMethod]
        public async Task ShouldFormatJsonResponse()
        {
            // arrange,
            const string unformattedJson = "{\"Foo\": \"Bar\"}";
            const string expectedJson = @"{
    ""Foo"": ""Bar""
}";

            // act
            var sut = await ExecuteFakeRequest(HttpMessageParts.All, JsonFormatting.IndentAll, unformattedJson);

            // assert
            sut.LogHistory[0].Should().Contain(expectedJson);
            sut.LogHistory[1].Should().Contain(expectedJson);
        }

        private static async Task<FakeLogger> ExecuteFakeRequest(HttpMessageParts? verbosity = null, JsonFormatting jsonFormatting = JsonFormatting.None, string responseContent = FakeHttpTraceHandler.FakeResponseContent)
        {
            var logger = new FakeLogger();

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.UseCookies = true;
            httpClientHandler.CookieContainer.Add(new Cookie(CookieName, CookieValue, "", new Uri(TestUri).Host));

            HttpContent fakeContent = new StringContent("{\"Foo\": \"Bar\"}", Encoding.Default, "application/json");
            var builder = new HttpHandlerBuilder(new FakeHttpTraceHandler(httpClientHandler, logger)
            {
                ResponseContent = responseContent,
                JsonFormatting = jsonFormatting
            });
            builder.AddHandler(new FakeHandler())
                .AddHandler(new SillyHandler())
                .AddHandler(new FakeHandler());

            if (verbosity != null)
                builder.SetHttpTracerVerbosity(verbosity.Value);

            var client = new HttpClient(builder.Build());
            await client.PostAsync(TestUri, fakeContent);
            return logger;
        }
    }
}