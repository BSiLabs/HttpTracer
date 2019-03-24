using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpTracer
{
    public class HttpTracerHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
        private const string LogMessageIndicatorPrefix = "====================";
        private const string LogMessageIndicatorSuffix = "====================";

        /// <summary>
        /// Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="HttpMessageHandler"/> and the default <see cref="DebugLogger"/>
        /// </summary>
        /// <param name="handler">User defined <see cref="HttpMessageHandler"/></param>
        public HttpTracerHandler(HttpMessageHandler handler) : this(handler, new DebugLogger())
        {
        }

        /// <summary>
        /// Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and the default <see cref="HttpClientHandler"/>
        /// </summary>
        /// <param name="logger">User defined <see cref="ILogger"/></param>
        public HttpTracerHandler(ILogger logger) : this(new HttpClientHandler(), logger)
        {
        }

        /// <summary>
        /// Constructs the <see cref="HttpTracerHandler"/> with the default <see cref="HttpClientHandler"/> and the default <see cref="DebugLogger"/>
        /// </summary>
        public HttpTracerHandler() : this(new HttpClientHandler(), new DebugLogger())
        {
        }

        /// <summary>
        /// Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/>
        /// </summary>
        /// <param name="handler">User defined <see cref="HttpMessageHandler"/></param>
        /// <param name="logger">User defined <see cref="ILogger"/></param>
        public HttpTracerHandler(HttpMessageHandler handler, ILogger logger)
        {
            InnerHandler = handler;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                await LogHttpRequest(request);
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                await LogHttpResponse(response);
                return response;
            }
            catch (Exception ex)
            {
                LogHttpException(request, ex);
                throw;
            }
        }

        protected static Task<string> GetRequestContent(HttpRequestMessage request)
            => request.Content.ReadAsStringAsync();


        protected static Task<string> GetResponseContent(HttpResponseMessage response)
            =>  response.Content.ReadAsStringAsync();


        protected void LogHttpException(HttpRequestMessage request, Exception ex)
        {
            var httpExceptionString = $@"{LogMessageIndicatorPrefix} HTTP EXCEPTION: [{request.Method}]{LogMessageIndicatorSuffix}
[{request.Method}] {request.RequestUri}
{ex}";
            _logger.Log(httpExceptionString);
        }

        protected async Task LogHttpRequest(HttpRequestMessage request)
        {
            var requestContent = string.Empty;
            if (request?.Content != null) requestContent = await GetRequestContent(request).ConfigureAwait(false);

            var httpLogString = $@"{LogMessageIndicatorPrefix}HTTP REQUEST: [{request?.Method}]{LogMessageIndicatorSuffix}
{request?.Method} {request?.RequestUri}
{request?.Headers.ToString().TrimEnd()}
content-type: application/json

{{
{requestContent}
}}";

            _logger.Log(httpLogString);
        }

        protected async Task LogHttpResponse(HttpResponseMessage response)
        {
            var responseContent = string.Empty;
            if (response?.Content != null) responseContent = await GetResponseContent(response).ConfigureAwait(false);

            const string succeeded = "SUCCEEDED";
            const string failed = "FAILED";

            var responseResult = response == null ? failed : (response.IsSuccessStatusCode ? $"{succeeded}: {(int)response.StatusCode} {response.StatusCode}" : $"{failed}: {(int)response.StatusCode} {response.StatusCode}");

            var httpLogString = $@"{LogMessageIndicatorPrefix}HTTP RESPONSE: [{responseResult}]{LogMessageIndicatorSuffix}
[{response?.RequestMessage?.Method}] {response?.RequestMessage?.RequestUri}
HttpResponse: {response}
HttpResponse.Content: 
{responseContent}";

            _logger.Log(httpLogString);
        }
    }
}
