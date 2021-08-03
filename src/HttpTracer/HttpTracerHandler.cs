using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        /// Default Json Formatting <see cref="JsonFormatting"/>
        /// </summary>
        public static JsonFormatting DefaultJsonFormatting { get; set; } = JsonFormatting.None;

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

        public static string LogMessageIndicatorPrefix { get; set; } = MessageIndicator;

        public static string LogMessageIndicatorSuffix { get; set; } = MessageIndicator;

        /// <summary>
        /// Instance verbosity bitmask, setting the instance verbosity overrides <see cref="DefaultVerbosity"/> <see cref="HttpMessageParts"/>
        /// </summary>
        public HttpMessageParts Verbosity
        {
            get => _verbosity == HttpMessageParts.Unspecified ? DefaultVerbosity : _verbosity;
            set => _verbosity = value;
        }

        /// <summary>
        /// Instance Json Formatting bitmask, setting the instance overrides <see cref="DefaultJsonFormatting"/> <see cref="JsonFormatting"/>
        /// </summary>
        public JsonFormatting JsonFormatting { get; set; } = JsonFormatting.None;

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        public HttpTracerHandler() : this(null, null)
        {
        }

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        /// <param name="handler">User defined <see cref="HttpMessageHandler"/></param>
        public HttpTracerHandler(HttpMessageHandler handler) : this(handler, null)
        {
        }

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        /// <param name="logger">User defined <see cref="ILogger"/></param>
        public HttpTracerHandler(ILogger logger) : this(null, logger)
        {
        }

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        /// <param name="verbosity">Instance verbosity bitmask, setting the instance verbosity overrides <see cref="DefaultVerbosity"/>  <see cref="HttpMessageParts"/></param>
        public HttpTracerHandler(HttpMessageParts verbosity) : this(null, null, verbosity)
        {
        }

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        /// <param name="handler">User defined <see cref="HttpMessageHandler"/></param>
        /// <param name="verbosity">Instance verbosity bitmask, setting the instance verbosity overrides <see cref="DefaultVerbosity"/>  <see cref="HttpMessageParts"/></param>
        public HttpTracerHandler(HttpMessageHandler handler, HttpMessageParts verbosity) : this(handler, null, verbosity)
        {
        }

        /// <summary> Constructs the <see cref="HttpTracerHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/></summary>
        /// <param name="logger">User defined <see cref="ILogger"/></param>
        /// <param name="verbosity">Instance verbosity bitmask, setting the instance verbosity overrides <see cref="DefaultVerbosity"/>  <see cref="HttpMessageParts"/></param>
        public HttpTracerHandler(ILogger logger, HttpMessageParts verbosity) : this(null, logger, verbosity)
        {
        }

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


                await LogHttpResponse(response, stopwatch.Elapsed).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                LogHttpException(request?.Method, request?.RequestUri, ex);
                throw;
            }
        }

        protected virtual string ProcessRequestUri(Uri uri) => uri?.ToString() ?? string.Empty;

        protected virtual string ProcessResponseLogHeading(HttpStatusCode statusCode, bool isSuccessStatusCode)
        {
            const string succeeded = "SUCCEEDED";
            const string failed = "FAILED";

            string responseResult;
            if (statusCode == default)
            {
                responseResult = failed;
            }
            else
            {
                responseResult = isSuccessStatusCode
                    ? $"{succeeded}: {(int) statusCode} {statusCode}"
                    : $"{failed}: {(int) statusCode} {statusCode}";
            }

            return responseResult;
        }

        protected virtual string ProcessRequestHeaders(HttpRequestHeaders requestHeaders) => $@"{requestHeaders.ToString().TrimEnd().TrimEnd('}').TrimStart('{')}";
        protected virtual string ProcessResponseHeaders(HttpResponseMessage responseHeaders) => responseHeaders.ToString();
        protected virtual string ProcessCookieHeader(CookieContainer cookieContainer, Uri requestRequestUri) => cookieContainer.GetCookieHeader(requestRequestUri);
        protected virtual Task<string> ProcessRequestBody(HttpContent requestContent) => requestContent?.ReadAsStringAsync() ?? Task.FromResult(string.Empty);
        protected virtual Task<string> ProcessResponseBody(HttpContent responseContent) => responseContent?.ReadAsStringAsync() ?? Task.FromResult(string.Empty);

        internal async Task LogHttpRequest(HttpRequestMessage request)
        {
            var sb = new StringBuilder();

            ConditionalAddRequestPrefix(request?.Method, request?.RequestUri, sb);
            ConditionalAddRequestHeaders(request?.Headers, sb);
            ConditionalAddCookies(request?.RequestUri, sb);
            await ConditionalAddRequestBody(request?.Content, sb);

            if (sb.Length > 0)
                _logger.Log(sb.ToString());
        }

        internal async Task LogHttpResponse(HttpResponseMessage response, TimeSpan duration)
        {
            var sb = new StringBuilder();
            ConditionalAddResponsePrefix(response?.StatusCode, response?.IsSuccessStatusCode, response?.RequestMessage?.Method, response?.RequestMessage?.RequestUri, sb);
            ConditionalAddResponseHeaders(response, sb);
            await ConditionalAddResponseBody(response?.Content, sb);
            ConditionalAddResponsePostfix(duration, sb);

            if (sb.Length > 0)
                _logger.Log(sb.ToString());
        }

        internal void LogHttpException(HttpMethod requestMethod, Uri requestUri, Exception ex)
        {
            var httpExceptionString = $@"{LogMessageIndicatorPrefix} HTTP EXCEPTION: [{requestMethod}]{LogMessageIndicatorSuffix}
{requestMethod} {requestUri}
{ex}";
            _logger.Log(httpExceptionString);
        }

        private void ConditionalAddResponsePostfix(TimeSpan duration, StringBuilder sb)
        {
            if (!Verbosity.HasFlag(HttpMessageParts.ResponseHeaders) && !Verbosity.HasFlag(HttpMessageParts.ResponseBody)) return;

            var httpResponsePostfix = string.Format(DefaultDurationFormat, duration);
            sb.AppendLine(httpResponsePostfix);
        }

        private async Task ConditionalAddResponseBody(HttpContent responseContent, StringBuilder sb)
        {
            if (!Verbosity.HasFlag(HttpMessageParts.ResponseBody)) return;

            var httpResponseContent = await ProcessResponseBody(responseContent).ConfigureAwait(false);

            if (JsonFormatting.HasFlag(JsonFormatting.IndentResponse) && responseContent?.Headers?.ContentType?.MediaType == JsonContentType)
            {
                httpResponseContent = PrettyFormatJson(httpResponseContent);
            }
            sb.AppendLine(httpResponseContent);
        }

        private void ConditionalAddResponseHeaders(HttpResponseMessage responseHeaders, StringBuilder sb)
        {
            if (!Verbosity.HasFlag(HttpMessageParts.ResponseHeaders)) return;

            var httpResponseHeaders = ProcessResponseHeaders(responseHeaders);
            sb.AppendLine(httpResponseHeaders);
        }

        private void ConditionalAddResponsePrefix(HttpStatusCode? responseStatusCode, bool? responseIsSuccessStatusCode, HttpMethod requestMessageMethod, Uri requestMessageRequestUri, StringBuilder sb)
        {
            if (!Verbosity.HasFlag(HttpMessageParts.ResponseHeaders) && !Verbosity.HasFlag(HttpMessageParts.ResponseBody)) return;

            var responseResult = ProcessResponseLogHeading(responseStatusCode ?? default, responseIsSuccessStatusCode ?? false);

            var httpResponsePrefix = $@"{LogMessageIndicatorPrefix}HTTP RESPONSE: [{responseResult}]{LogMessageIndicatorSuffix}";
            sb.AppendLine(httpResponsePrefix);

            var httpRequestMethodUri = $@"{requestMessageMethod} {ProcessRequestUri(requestMessageRequestUri)}";
            sb.AppendLine(httpRequestMethodUri);
        }

        private async Task ConditionalAddRequestBody(HttpContent requestContent, StringBuilder sb)
        {
            if (!Verbosity.HasFlag(HttpMessageParts.RequestBody)) return;

            var httpRequestBody = await ProcessRequestBody(requestContent).ConfigureAwait(false);

            if (JsonFormatting.HasFlag(JsonFormatting.IndentRequest) && requestContent?.Headers?.ContentType?.MediaType == JsonContentType)
            {
                httpRequestBody = PrettyFormatJson(httpRequestBody);
            }
            sb.AppendLine(httpRequestBody);
        }


        private void ConditionalAddRequestHeaders(HttpRequestHeaders requestHeaders, StringBuilder sb)
        {
            if (!Verbosity.HasFlag(HttpMessageParts.RequestHeaders)) return;

            var httpErrorRequestHeaders = ProcessRequestHeaders(requestHeaders);
            sb.AppendLine(httpErrorRequestHeaders);
        }

        private void ConditionalAddRequestPrefix(HttpMethod requestMethod, Uri requestUri, StringBuilder sb)
        {
            if (!Verbosity.HasFlag(HttpMessageParts.RequestHeaders) && !Verbosity.HasFlag(HttpMessageParts.RequestBody)) return;

            var httpRequestPrefix = $"{LogMessageIndicatorPrefix}HTTP REQUEST: [{requestMethod}]{LogMessageIndicatorSuffix}";
            sb.AppendLine(httpRequestPrefix);

            var httpRequestMethodUri = $@"{requestMethod} {ProcessRequestUri(requestUri)}";
            sb.AppendLine(httpRequestMethodUri);
        }

        private void ConditionalAddCookies(Uri requestUri, StringBuilder sb)
        {
            if (!Verbosity.HasFlag(HttpMessageParts.RequestCookies) || InnerHandler is not HttpClientHandler httpClientHandler || requestUri == default) return;

            var cookieHeader = ProcessCookieHeader(httpClientHandler.CookieContainer, requestUri);
            if (string.IsNullOrWhiteSpace(cookieHeader)) return;

            sb.AppendLine($"{Environment.NewLine}Cookie: {cookieHeader}");
        }
        
        private static string PrettyFormatJson(string json) {

            var indentation = 0;
            var quoteCount = 0;
            var result = json.Select(ch => new {ch, quotes = ch == '"' ? quoteCount++ : quoteCount})
                .Select(t => new {t, lineBreak = t.ch == ',' && t.quotes % 2 == 0 ? t.ch + Environment.NewLine + string.Concat(Enumerable.Repeat(JsonIndentationString, indentation)) : null})
                .Select(t => new {t, openChar = t.t.ch == '{' || t.t.ch == '[' ? t.t.ch + Environment.NewLine + string.Concat(Enumerable.Repeat(JsonIndentationString, ++indentation)) : t.t.ch.ToString()})
                .Select(t => new {t, closeChar = t.t.t.ch == '}' || t.t.t.ch == ']' ? Environment.NewLine + string.Concat(Enumerable.Repeat(JsonIndentationString, --indentation)) + t.t.t.ch : t.t.t.ch.ToString()})
                .Select(t => t.t.t.lineBreak ?? (t.t.openChar.Length > 1 ? t.t.openChar : t.closeChar));

            return String.Concat(result);
        }

        private readonly ILogger _logger;
        private HttpMessageParts _verbosity;
        private const string JsonContentType = "application/json";
        private const string MessageIndicator = " ==================== ";
        private const string JsonIndentationString = "    ";
    }
}