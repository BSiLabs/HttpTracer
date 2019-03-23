using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpTracer
{
    public class HttpTracerHandler : DelegatingHandler
    {
        /// <summary>
        /// Sets the verbosity for the HttpTracer logging
        /// Trace - full requests and responses for all http requests
        /// Debug - full requests, responses without body
        /// Information - full request and response for errors, request and response method/url/header for all requests
        /// Critical - full request and response for errors
        /// Error - request method/url/header and full response for errors only
        /// </summary>
        public LogLevel Verbosity { get; set; } = LogLevel.Trace;
        
        private readonly ILogger _logger;
        
        private const string MessageIndicator = " ==================== ";
        public static string LogMessageIndicatorPrefix = MessageIndicator;
        public static string LogMessageIndicatorSuffix = MessageIndicator;

        /// <summary>
        /// Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="HttpMessageHandler"/> and the default <see cref="ConsoleLogger"/>
        /// </summary>
        /// <param name="handler">User defined <see cref="HttpMessageHandler"/></param>
        public HttpTracerHandler(HttpMessageHandler handler) : this(handler, new ConsoleLogger())
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
        /// Constructs the <see cref="HttpTracerHandler"/> with the default <see cref="HttpClientHandler"/> and the default <see cref="ConsoleLogger"/>
        /// </summary>
        public HttpTracerHandler() : this(new HttpClientHandler())
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
                await LogHttpRequest(request).ConfigureAwait(false);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                await LogHttpResponse(response, stopwatch.ElapsedMilliseconds).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                LogHttpException(request, ex).FireAndForget();
                throw;
            }
        }

        protected async Task LogHttpRequest(HttpRequestMessage request)
        {
            if (Verbosity > LogLevel.Information) return;
            
            string httpRequestString;
            if (Verbosity <= LogLevel.Debug)
            {
                httpRequestString = await GetFullRequestString(request);
            }
            else
            {
                httpRequestString = GetSimpleRequestString(request);
            }

            _logger.Log(httpRequestString);

            _logger.Log(httpRequestString);
        }

        protected async Task LogHttpResponse(HttpResponseMessage response, long elapsedMilliseconds)
        {
            if (Verbosity >= LogLevel.Critical && response.IsSuccessStatusCode) return;
            
            var responseContent = string.Empty;
            if (response?.Content != null)
            {
                if (Verbosity == LogLevel.Trace || (!response.IsSuccessStatusCode && Verbosity < LogLevel.Error)) 
                    responseContent = await GetResponseContent(response).ConfigureAwait(false);
            }

            const string succeeded = "SUCCEEDED";
            const string failed = "FAILED";

            var responseResult = response == null ? failed : (response.IsSuccessStatusCode ? $"{succeeded}: {(int)response.StatusCode} {response.StatusCode}" : $"{failed}: {(int)response.StatusCode} {response.StatusCode}");

            var httpLogString =
                $@"{LogMessageIndicatorPrefix}HTTP RESPONSE: [{responseResult}]{LogMessageIndicatorSuffix}
{response?.RequestMessage?.Method} {response?.RequestMessage?.RequestUri}
HttpResponse: {response}";

            if (Verbosity == LogLevel.Trace)
            {
                httpLogString +=
                    $@"
HttpResponse.Content: 
{responseContent}
{elapsedMilliseconds}ms";
            }

            _logger.Log(httpLogString);
        }

        protected async Task LogHttpException(HttpRequestMessage request, Exception ex)
        {
            string httpRequestString;
            if (Verbosity < LogLevel.Error)
            {
                httpRequestString = await GetFullRequestString(request);
            }
            else
            {
                httpRequestString = GetSimpleRequestString(request);
            }

            _logger.Log(httpRequestString);
            
            var httpExceptionString = $@"{LogMessageIndicatorPrefix}HTTP EXCEPTION: [{request.Method}]{LogMessageIndicatorSuffix}
{request.Method} {request.RequestUri}
{ex}";
            _logger.Log(httpExceptionString);
        }

        private static async Task<string> GetFullRequestString(HttpRequestMessage request)
        {
            var httpLogString = GetSimpleRequestString(request);
            httpLogString += await GetRequestBodyString(request);
            return httpLogString;
        }

        private static string GetSimpleRequestString(HttpRequestMessage request)
        {
            var httpLogString =
                $@"{LogMessageIndicatorPrefix}HTTP REQUEST: [{request?.Method}]{LogMessageIndicatorSuffix}
{request?.Method} {request?.RequestUri}
{request?.Headers.ToString().TrimEnd()}
";
            return httpLogString;
        }

        private static async Task<string> GetRequestBodyString(HttpRequestMessage request)
        {
            var requestContent = string.Empty;
            if (request?.Content != null) requestContent = await GetRequestContent(request).ConfigureAwait(false);
            return requestContent;
        }

        protected static Task<string> GetRequestContent(HttpRequestMessage request) =>  request.Content.ReadAsStringAsync();

        protected static Task<string> GetResponseContent(HttpResponseMessage response) => response.Content.ReadAsStringAsync();
    }
}
