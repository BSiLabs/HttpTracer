using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpTracer.Tests.Fakes
{
    public class SillyHandler : DelegatingHandler
    {
        public SillyHandler()
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
}