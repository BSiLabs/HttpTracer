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
        private HttpTracerHandler _ourHandler = new HttpTracerHandler();

        private IList<DelegatingHandler> _otherHandlersList = new List<DelegatingHandler>();

        public HttpHandlerBuilder Add(DelegatingHandler handler)
        {
            _otherHandlersList.Add(handler);
            return this;
        }

        public void Build()
        {
            //receive parameters from client

            //create the chain of 
            foreach (var otherHandler in _otherHandlersList)
            {
                //when 
            }
        }
    }

	public class HttpTracerHandler : DelegatingHandler
	{
		private readonly ILogger _logger = new DebugLogger();

	    public HttpTracerHandler(ILogger logger):this()
	    {
	        _logger = logger;
	    }

	    public HttpTracerHandler()
	    {
            InnerHandler = new HttpClientHandler();
        }

	    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	    {
	        await LogHttpRequest(request);

            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogHttpException(request, ex);
                throw;
            }

            await LogHttpResponse(response);

            return response;
	    }

		private async Task LogHttpRequest(HttpRequestMessage request)
		{
			var requestContent = string.Empty;
			if (request?.Content != null)
			{
				requestContent = await GetRequestContent(request);
			}

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
			if (response?.Content != null)
			{
				responseContent = await GetResponseContent(response);
			}

			var responseResult = response?.IsSuccessStatusCode ?? false ? "SUCCEEDED" : "FAILED";

			var httpLogString = $@"==================== HTTP RESPONSE: [{responseResult}] ====================
[{response?.RequestMessage?.Method}] {response?.RequestMessage?.RequestUri}
HttpResponse: {response}
HttpResponse.Content: {responseContent}";

			_logger.Log(httpLogString);
		}

		private void LogHttpException(HttpRequestMessage request, Exception ex)
		{
			var httpExceptionString = $@"==================== HTTP EXCEPTION: [ {request.Method} ]====================
[{request.Method}] {request.RequestUri}
{ex}";
			_logger.Log(httpExceptionString);
		}

		private static async Task<string> GetResponseContent(HttpResponseMessage response)
		{
			return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		}
		private static async Task<string> GetRequestContent(HttpRequestMessage request)
		{
			return await request.Content.ReadAsStringAsync().ConfigureAwait(false);
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
