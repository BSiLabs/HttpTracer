using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpTracer.Tests.Fakes
{
    public class SillyHandler : DelegatingHandler
    {
        public static readonly string SillyHeader = $"{HeaderKey}: {HeaderValue}";
        public const string HeaderKey = "SILLY-HEADER";
        public const string HeaderValue = "SILLY VALUE";
        
        public SillyHandler()
        {
            InnerHandler = new HttpClientHandler();
        }
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