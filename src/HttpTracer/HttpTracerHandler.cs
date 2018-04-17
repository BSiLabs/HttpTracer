using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpTracer
{
    public class HttpHandlerBuilder
    {
        private readonly IList<HttpMessageHandler> _handlersList = new List<HttpMessageHandler>();
        private readonly HttpTracerHandler _ourHandler = new HttpTracerHandler();

        /// <summary>
        /// Adds a <see cref="HttpMessageHandler"/> to the chain of handlers.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public HttpHandlerBuilder AddHandler(HttpMessageHandler handler)
        {
            if (handler is HttpTracerHandler) throw new ArgumentException($"Can't add handler of type {nameof(HttpTracerHandler)}.");

            if (_handlersList.Any())
                ((DelegatingHandler)_handlersList.LastOrDefault()).InnerHandler = handler;

            _handlersList.Add(handler);
            return this;
        }

        /// <summary>
        /// Adds <see cref="HttpTracerHandler"/> as the last link of the chain.
        /// </summary>
        /// <returns></returns>
        public HttpMessageHandler Build()
        {
            if (_handlersList.Any())
                ((DelegatingHandler)_handlersList.LastOrDefault()).InnerHandler = _ourHandler;
            else
                return _ourHandler;

            return _handlersList.FirstOrDefault();
        }
    }

    public class HttpTracerHandler : DelegatingHandler
    {
        private readonly ILogger _logger;

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
                LogHttpRequest(request).FireAndForget();
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                LogHttpResponse(response).FireAndForget();
                return response;
            }
            catch (Exception ex)
            {
                LogHttpException(request, ex).FireAndForget();
                throw;
            }
        }

        private static Task<string> GetRequestContent(HttpRequestMessage request)
        {
            return request.Content.ReadAsStringAsync();
        }

        private static Task<string> GetResponseContent(HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync();
        }

        private Task LogHttpException(HttpRequestMessage request, Exception ex)
        {
            var httpExceptionString = $@"==================== HTTP EXCEPTION: [ {request.Method} ]====================
[{request.Method}] {request.RequestUri}
{ex}";
            _logger.Log(httpExceptionString);
            return Task.CompletedTask;
        }

        private async Task LogHttpRequest(HttpRequestMessage request)
        {
            var requestContent = string.Empty;
            if (request?.Content != null) requestContent = await GetRequestContent(request).ConfigureAwait(false);

            var httpLogString = $@"==================== HTTP REQUEST: [ {request?.Method} ]====================
{request?.RequestUri}
Headers:
{{
{request?.Headers.ToString().TrimEnd()}
}}
HttpRequest.Content: 
{requestContent}";

            _logger.Log(httpLogString);
        }

        private async Task LogHttpResponse(HttpResponseMessage response)
        {
            var responseContent = string.Empty;
            if (response?.Content != null) responseContent = await GetResponseContent(response).ConfigureAwait(false);

            var responseResult = response?.IsSuccessStatusCode ?? false ? "SUCCEEDED" : "FAILED";

            var httpLogString = $@"==================== HTTP RESPONSE: [{responseResult}] ====================
[{response?.RequestMessage?.Method}] {response?.RequestMessage?.RequestUri}
HttpResponse: {response}
HttpResponse.Content: {responseContent}";

            _logger.Log(httpLogString);
        }
    }

    public interface ILogger
    {
        void Log(string message);
    }

    public class DebugLogger : ILogger
    {
        public void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
