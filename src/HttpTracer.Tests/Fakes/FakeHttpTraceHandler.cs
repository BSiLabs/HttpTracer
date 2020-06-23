using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpTracer.Logger;

namespace HttpTracer.Tests.Fakes
{
    public class FakeHttpTraceHandler : HttpTracerHandler
    {
        public const string FakeResponseContent = "Response Content";
        public FakeHttpTraceHandler(HttpClientHandler httpClientHandler, ILogger logger) : base(httpClientHandler, logger) {}

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                await LogHttpRequest(request);
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(FakeResponseContent)};
                await LogHttpResponse(response, TimeSpan.FromMilliseconds(25));
                return response;
            }
            catch (Exception ex)
            {
                LogHttpException(request, ex);
                throw;
            }
        }
    }
}