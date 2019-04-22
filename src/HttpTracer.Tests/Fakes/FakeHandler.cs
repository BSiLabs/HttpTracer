using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpTracer.Tests.Fakes
{
    public class FakeHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);
            request.Headers.Add("FAKE-HEADER", "FAKE VALUE");

            await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            return new HttpResponseMessage();
        }
    }
}