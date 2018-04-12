using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpTracer
{
    public class HttpHandlerBuilder
    {
        private readonly IList<DelegatingHandler> _otherHandlersList = new List<DelegatingHandler>();
        private HttpTracerHandler _ourHandler = new HttpTracerHandler();

        public HttpHandlerBuilder AddDelegatingHandler(DelegatingHandler handler)
        {
            _otherHandlersList.Add(handler);
            return this;
        }

        public HttpTracerHandler Build()
        {
            // Our handler needs to be the root
            //var pipeline = new MyHandler1
            //{
            //    InnerHandler = new HttpTracerHandler()
            //};
            
            //create the chain of handlers, with our handler as the root
            foreach (var otherHandler in _otherHandlersList)
            {
                //when 
            }

            return _ourHandler;
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
            await LogHttpRequest(request).ConfigureAwait(false);

            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                await LogHttpResponse(response).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                LogHttpException(request, ex);
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

        private void LogHttpException(HttpRequestMessage request, Exception ex)
        {
            var httpExceptionString = $@"\n==================== HTTP EXCEPTION: [ {request.Method} ]====================
[{request.Method}] {request.RequestUri}
{ex}\n";
            _logger.Log(httpExceptionString);
        }

        private async Task LogHttpRequest(HttpRequestMessage request)
        {
            var requestContent = string.Empty;
            if (request?.Content != null) requestContent = await GetRequestContent(request).ConfigureAwait(false);

            var httpLogString = $@"\n==================== HTTP REQUEST: [ {request?.Method} ]====================
{request?.RequestUri}
Headers:
{{
{request?.Headers.ToString().TrimEnd()}
}}
HttpRequest.Content: 
{requestContent}\n";

            _logger.Log(httpLogString);
        }

        private async Task LogHttpResponse(HttpResponseMessage response)
        {
            var responseContent = string.Empty;
            if (response?.Content != null) responseContent = await GetResponseContent(response).ConfigureAwait(false);

            var responseResult = response?.IsSuccessStatusCode ?? false ? "SUCCEEDED" : "FAILED";

            var httpLogString = $@"\n==================== HTTP RESPONSE: [{responseResult}] ====================
[{response?.RequestMessage?.Method}] {response?.RequestMessage?.RequestUri}
HttpResponse: {response}
HttpResponse.Content: {responseContent}\n";

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
