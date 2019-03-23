using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace HttpTracer
{
    public class HttpHandlerBuilder
    {
        private readonly IList<HttpMessageHandler> _handlersList = new List<HttpMessageHandler>();
        private readonly HttpTracerHandler _rootHandler;

        /// <summary>
        /// Underlying instance of the <see cref="T:HttpTracer.HttpHandlerBuilder"/> class.
        /// </summary>
        public HttpTracerHandler HttpTracerHandler => _rootHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:HttpTracer.HttpHandlerBuilder"/> class.
        /// </summary>
        public HttpHandlerBuilder()
        {
            _rootHandler = new HttpTracerHandler();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:HttpTracer.HttpHandlerBuilder"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        public HttpHandlerBuilder(ILogger logger)
        {
            _rootHandler = new HttpTracerHandler(logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:HttpTracer.HttpHandlerBuilder"/> class.
        /// </summary>
        /// <param name="tracerHandler">Tracer handler.</param>
        public HttpHandlerBuilder(HttpTracerHandler tracerHandler)
        {
            _rootHandler = tracerHandler;
        }

        /// <summary>
        /// Adds a <see cref="HttpMessageHandler"/> to the chain of handlers.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public HttpHandlerBuilder AddHandler(DelegatingHandler handler)
        {
            if (handler is HttpTracerHandler) throw new ArgumentException($"Can't add handler of type {nameof(HttpTracerHandler)}.");

            if (_handlersList.LastOrDefault() is DelegatingHandler delegatingHandler)
                delegatingHandler.InnerHandler = handler;

            _handlersList.Add(handler);
            return this;
        }

        /// <summary>
        /// Adds <see cref="HttpTracerHandler"/> as the last link of the chain.
        /// </summary>
        /// <returns></returns>
        public HttpMessageHandler Build()
        {
            if (!_handlersList.Any()) return _rootHandler;
            
            if (_handlersList.LastOrDefault() is DelegatingHandler delegatingHandler)
                delegatingHandler.InnerHandler = _rootHandler;
            return _handlersList.FirstOrDefault();
        }

        /// <summary>
        /// Sets the verbosity for the underlying <see cref="HttpTracerHandler"/>
        /// </summary>
        /// <param name="verbosity"></param>
        /// <returns></returns>
        public HttpHandlerBuilder SetHttpTracerVerbosity(LogLevel verbosity)
        {
            _rootHandler.Verbosity = verbosity;
            return this;
        }
    }
}