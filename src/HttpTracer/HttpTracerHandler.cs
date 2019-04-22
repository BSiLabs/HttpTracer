using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpTracer.Logger;
using System.Text;

namespace HttpTracer
{
    public class HttpTracerHandler : DelegatingHandler
    {
        public static HttpMessageParts DefaultVerbosity { get; set; } = HttpMessageParts.All;

        private HttpMessageParts _verbosity = HttpMessageParts.Unspecified;
        public HttpMessageParts Verbosity
        {
            get => _verbosity == HttpMessageParts.Unspecified ? DefaultVerbosity : _verbosity;
            set => _verbosity = value;
        }

        private readonly ILogger _logger;

        private const string MessageIndicator = " ==================== ";
        public static string LogMessageIndicatorPrefix = MessageIndicator;
        public static string LogMessageIndicatorSuffix = MessageIndicator;

        /// <summary>
        /// Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/>
        /// </summary>
        /// <param name="handler">User defined <see cref="HttpMessageHandler"/></param>
        /// <param name="logger">User defined <see cref="ILogger"/></param>
        public HttpTracerHandler(HttpMessageHandler handler = null, ILogger logger = null, HttpMessageParts verbosity = HttpMessageParts.Unspecified)
        {
            InnerHandler = handler ?? new HttpClientHandler();
            _logger = logger ?? new ConsoleLogger();
            _verbosity = verbosity;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                await LogHttpRequest(request).ConfigureAwait(false);
                
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                
                if (!response.IsSuccessStatusCode) await LogHttpErrorRequest(request);
                
                await LogHttpResponse(response, stopwatch.ElapsedMilliseconds).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                LogHttpException(request, ex);
                throw;
            }
        }

        private async Task LogHttpErrorRequest(HttpRequestMessage request)
        {
            var sb = new StringBuilder();
            var httpErrorRequestPrefix =
                $"{LogMessageIndicatorPrefix}HTTP ERROR REQUEST: [{request?.Method}]{LogMessageIndicatorSuffix}";
            sb.AppendLine(httpErrorRequestPrefix);
            
            var httpErrorRequestHeaders = GetRequestHeaders(request);
            sb.AppendLine(httpErrorRequestHeaders);
            
            var httpErrorRequestBody = await GetRequestBody(request);
            sb.AppendLine(httpErrorRequestBody);
            _logger.Log(sb.ToString());
        }

        protected virtual async Task LogHttpRequest(HttpRequestMessage request)
        {
            var sb = new StringBuilder();
            if (Verbosity.HasFlag(HttpMessageParts.RequestHeaders) || Verbosity.HasFlag(HttpMessageParts.RequestBody))
            {
                var httpRequestPrefix =
                    $"{LogMessageIndicatorPrefix}HTTP REQUEST: [{request?.Method}]{LogMessageIndicatorSuffix}";
                sb.AppendLine(httpRequestPrefix);
            }

            if (Verbosity.HasFlag(HttpMessageParts.RequestHeaders))
            {
                var httpErrorRequestHeaders = GetRequestHeaders(request);
                sb.AppendLine(httpErrorRequestHeaders);
            }

            if (Verbosity.HasFlag(HttpMessageParts.RequestBody))
            {
                var httpErrorRequestBody = await GetRequestBody(request);
                sb.AppendLine(httpErrorRequestBody);
            }
            _logger.Log(sb.ToString());
        }

        protected virtual async Task LogHttpResponse(HttpResponseMessage response, long elapsedMilliseconds)
        {
            var sb = new StringBuilder();
            if (Verbosity.HasFlag(HttpMessageParts.ResponseHeaders) || Verbosity.HasFlag(HttpMessageParts.ResponseBody))
            {
                var responseResult = GetResponseLogHeading(response);

                var httpResponsePrefix =
                    $@"{LogMessageIndicatorPrefix}HTTP RESPONSE: [{responseResult}]{LogMessageIndicatorSuffix}";
                sb.AppendLine(httpResponsePrefix);
            }

            if (Verbosity.HasFlag(HttpMessageParts.ResponseHeaders))
            {
                var httpResponseHeaders = $@"
{response?.RequestMessage?.Method} {response?.RequestMessage?.RequestUri}
{response}";
                sb.AppendLine(httpResponseHeaders);
            }

            if (Verbosity.HasFlag(HttpMessageParts.ResponseBody))
            {
                var responseContent = await GetResponseBody(response).ConfigureAwait(false);

                var httpResponseContent =
                    $@"
{responseContent}";
                sb.AppendLine(httpResponseContent);
            }

            if (Verbosity.HasFlag(HttpMessageParts.ResponseHeaders) || Verbosity.HasFlag(HttpMessageParts.ResponseBody))
            {
                var httpResponsePostfix = $"{elapsedMilliseconds}ms";
                sb.AppendLine(httpResponsePostfix);
            }
            _logger.Log(sb.ToString());
        }

        private string GetResponseLogHeading(HttpResponseMessage response)
        {
            const string succeeded = "SUCCEEDED";
            const string failed = "FAILED";

            string responseResult;
            if (response == null)
                responseResult = failed;
            else
                responseResult = response.IsSuccessStatusCode
                    ? $"{succeeded}: {(int) response.StatusCode} {response.StatusCode}"
                    : $"{failed}: {(int) response.StatusCode} {response.StatusCode}";
            return responseResult;
        }

        protected void LogHttpException(HttpRequestMessage request, Exception ex)
        {
            var httpExceptionString = $@"{LogMessageIndicatorPrefix} HTTP EXCEPTION: [{request.Method}]{LogMessageIndicatorSuffix}
{request.Method} {request.RequestUri}
{ex}";
            _logger.Log(httpExceptionString);
        }

        private string GetRequestHeaders(HttpRequestMessage request) =>
            !Verbosity.HasFlag(HttpMessageParts.RequestHeaders) ? string.Empty : $@"{request?.Method} {request?.RequestUri}
{request?.Headers.ToString().TrimEnd()}
";

        protected Task<string> GetRequestBody(HttpRequestMessage request) => request?.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty);

        protected Task<string> GetResponseBody(HttpResponseMessage response) => response?.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty);
    }
}
