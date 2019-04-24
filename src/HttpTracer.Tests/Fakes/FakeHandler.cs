using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpTracer.Tests.Fakes
{
    public class FakeHandler : DelegatingHandler
    {
        public static readonly string FakeHeader = $"{HeaderKey}: {HeaderValue}";
        public const string HeaderKey = "FAKE-HEADER";
        public const string HeaderValue = "FAKE VALUE";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);
            request.Headers.Add(HeaderKey, HeaderValue);

            await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            return new HttpResponseMessage();
        }
    }
}