using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpTracer.Logger;
using System.Text;

namespace HttpTracer
{
    public class HttpTracerHandler : DelegatingHandler
    {
        /// <summary>
        /// Default verbosity bitmask <see cref="HttpMessageParts"/>
        /// </summary>
        public static HttpMessageParts DefaultVerbosity { get; set; } = HttpMessageParts.All;

        /// <summary>
        /// Duration string format. Defaults to "Duration: {0:ss\\:fffffff}"
        /// </summary>
        /// <remarks>
        /// <para>
        /// Receives a <see cref="TimeSpan"/> at the [0] index.
        /// </para>
        /// <para>
        /// See <a href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings">https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings</a> for more details on TimeSpan formatting.
        /// </para>
        /// </remarks>
        public static string DefaultDurationFormat { get; set; } = "Duration: {0:ss\\:fffffff}";

        private HttpMessageParts _verbosity;
        
        /// <summary>
        /// Instance verbosity bitmask, setting the instance verbosity overrides <see cref="DefaultVerbosity"/> <see cref="HttpMessageParts"/>
        /// </summary>
        public HttpMessageParts Verbosity
        {
            get => _verbosity == HttpMessageParts.Unspecified ? DefaultVerbosity : _verbosity;
            set => _verbosity = value;
        }

        private readonly ILogger _logger;

        // ReSharper disable InconsistentNaming
        private const string MessageIndicator = " ==================== ";
        // ReSharper restore InconsistentNaming
        public static string LogMessageIndicatorPrefix = MessageIndicator;
        public static string LogMessageIndicatorSuffix = MessageIndicator;

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        public HttpTracerHandler() : this(null,null) { }

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        /// <param name="handler">User defined <see cref="HttpMessageHandler"/></param>
        public HttpTracerHandler(HttpMessageHandler handler) : this(handler,null) { }

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        /// <param name="logger">User defined <see cref="ILogger"/></param>
        public HttpTracerHandler(ILogger logger) : this(null,logger) { }

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        /// <param name="verbosity">Instance verbosity bitmask, setting the instance verbosity overrides <see cref="DefaultVerbosity"/>  <see cref="HttpMessageParts"/></param>
        public HttpTracerHandler(HttpMessageParts verbosity) : this(null,null,verbosity) { }

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        /// <param name="handler">User defined <see cref="HttpMessageHandler"/></param>
        /// <param name="verbosity">Instance verbosity bitmask, setting the instance verbosity overrides <see cref="DefaultVerbosity"/>  <see cref="HttpMessageParts"/></param>
        public HttpTracerHandler(HttpMessageHandler handler, HttpMessageParts verbosity) : this(handler,null, verbosity) { }

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        /// <param name="logger">User defined <see cref="ILogger"/></param>
        /// <param name="verbosity">Instance verbosity bitmask, setting the instance verbosity overrides <see cref="DefaultVerbosity"/>  <see cref="HttpMessageParts"/></param>
        public HttpTracerHandler(ILogger logger, HttpMessageParts verbosity) : this(null,logger, verbosity) { }
        
        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        /// <param name="handler">User defined <see cref="HttpMessageHandler"/></param>
        /// <param name="logger">User defined <see cref="ILogger"/></param>
        /// <param name="verbosity">Instance verbosity bitmask, setting the instance verbosity overrides <see cref="DefaultVerbosity"/>  <see cref="HttpMessageParts"/></param>
        public HttpTracerHandler(HttpMessageHandler handler, ILogger logger, HttpMessageParts verbosity = HttpMessageParts.Unspecified)
        {
            InnerHandler = handler ?? new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
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
                
                await LogHttpResponse(response, stopwatch.Elapsed).ConfigureAwait(false);
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
            
            if(sb.Length>0)
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
                
                var httpRequestMethodUri = $@"{request?.Method} {request?.RequestUri}";
                sb.AppendLine(httpRequestMethodUri);
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
            
            if (sb.Length > 0)
                _logger.Log(sb.ToString());
        }

        protected virtual async Task LogHttpResponse(HttpResponseMessage response, TimeSpan duration)
        {
            var sb = new StringBuilder();
            if (Verbosity.HasFlag(HttpMessageParts.ResponseHeaders) || Verbosity.HasFlag(HttpMessageParts.ResponseBody))
            {
                var responseResult = GetResponseLogHeading(response);

                var httpResponsePrefix =
                    $@"{LogMessageIndicatorPrefix}HTTP RESPONSE: [{responseResult}]{LogMessageIndicatorSuffix}";
                sb.AppendLine(httpResponsePrefix);

                var httpRequestMethodUri = $@"{response?.RequestMessage?.Method} {response?.RequestMessage?.RequestUri}";
                sb.AppendLine(httpRequestMethodUri);
            }

            if (Verbosity.HasFlag(HttpMessageParts.ResponseHeaders))
            {
                var httpResponseHeaders = $@"{response}";
                sb.AppendLine(httpResponseHeaders);
            }

            if (Verbosity.HasFlag(HttpMessageParts.ResponseBody))
            {
                var httpResponseContent = await GetResponseBody(response).ConfigureAwait(false);
                sb.AppendLine(httpResponseContent);
            }

            if (Verbosity.HasFlag(HttpMessageParts.ResponseHeaders) || Verbosity.HasFlag(HttpMessageParts.ResponseBody))
            {
                var httpResponsePostfix = string.Format(DefaultDurationFormat, duration);
                sb.AppendLine(httpResponsePostfix);
            }
            
            if (sb.Length > 0)
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

        private string GetRequestHeaders(HttpRequestMessage request)
        {
            string httpRequestHeaders = string.Empty;

            if (request is null)
                return httpRequestHeaders;
            
            if (Verbosity.HasFlag(HttpMessageParts.RequestHeaders))
                httpRequestHeaders = $@"{request.Headers.ToString().TrimEnd().TrimEnd('}').TrimStart('{')}";

            if (Verbosity.HasFlag(HttpMessageParts.RequestCookies)
                && InnerHandler is HttpClientHandler httpClientHandler)
            {
                if (!httpClientHandler.UseCookies) return httpRequestHeaders;
                var cookieHeader = httpClientHandler.CookieContainer.GetCookieHeader(request.RequestUri);
                if (!string.IsNullOrWhiteSpace(cookieHeader))
                {
                    httpRequestHeaders += $"{Environment.NewLine}Cookie: {cookieHeader}";
                }
            }
            return httpRequestHeaders;
        }

        protected Task<string> GetRequestBody(HttpRequestMessage request) => request?.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty);

        protected Task<string> GetResponseBody(HttpResponseMessage response) => response?.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty);
    }
}
