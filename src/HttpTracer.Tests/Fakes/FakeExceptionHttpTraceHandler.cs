using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpTracer.Logger;

namespace HttpTracer.Tests.Fakes
{
    public class FakeExceptionHttpTraceHandler : HttpTracerHandler
    {
        public FakeExceptionHttpTraceHandler(ILogger logger) : base(null, logger) {}

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                await LogHttpRequest(request);
                throw new ApplicationException("It done broke.");
            }
            catch (Exception ex)
            {
                LogHttpException(request.Method, request.RequestUri, ex);
                throw;
            }
        }
    }
}