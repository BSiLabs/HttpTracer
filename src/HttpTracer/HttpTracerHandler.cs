using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpTracer
{
	public class HttpTracerHandler : DelegatingHandler
	{
		private readonly DebugLogger _loggerFacade = new DebugLogger();

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

			var httpLogString = $@"==================== HTTP REQUEST [ {request?.Method} ]====================
								{request?.RequestUri}
								Headers:
								{{
								{request?.Headers.ToString().TrimEnd()}
								}}
								HttpRequest.Content: 
								{requestContent}";

			_loggerFacade.Log(httpLogString);
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

			_loggerFacade.Log(httpLogString);
		}

		private void LogHttpException(HttpRequestMessage request, Exception ex)
		{
			var httpExceptionString = $@"==================== HTTP EXCEPTION [ {request.Method} ]====================
										[{request.Method}] {request.RequestUri}
										{ex}";
			_loggerFacade.Log(httpExceptionString);
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

	public class DebugLogger
	{
		public void Log(string message)
		{
			var messageToLog = string.Format(CultureInfo.InvariantCulture, DateTime.Now.ToString(CultureInfo.InvariantCulture), message);

			Debug.WriteLine(messageToLog);
		}
	}
}
