using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HttpTracer.Logger;

namespace HttpTracer
{
    public class HttpHandlerBuilder
    {
        private readonly IList<DelegatingHandler> _handlersList = new List<DelegatingHandler>();
        private readonly HttpTracerHandler _rootHandler;

        /// <summary>
        /// Underlying instance of the <see cref="T:HttpTracer.HttpHandlerBuilder"/> class.
        /// </summary>
        public HttpTracerHandler HttpTracerHandler => _rootHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:HttpTracer.HttpHandlerBuilder"/> class.
        /// </summary>
        public HttpHandlerBuilder() : this (new HttpTracerHandler(null, null)) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="T:HttpTracer.HttpHandlerBuilder"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        public HttpHandlerBuilder(ILogger logger) : this (new HttpTracerHandler(null, logger)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:HttpTracer.HttpHandlerBuilder"/> class.
        /// </summary>
        /// <param name="innerHandler">HttpClientHandler.</param>
        public HttpHandlerBuilder(HttpClientHandler innerHandler) : this (new HttpTracerHandler(innerHandler, null)) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="T:HttpTracer.HttpHandlerBuilder"/> class.
        /// </summary>
        /// <param name="innerHandler">HttpClientHandler.</param>
        /// <param name="logger">Logger.</param>
        public HttpHandlerBuilder(HttpClientHandler innerHandler, ILogger logger) : this (new HttpTracerHandler(innerHandler, logger)) { }

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

            if (_handlersList.Any())
                _handlersList.Last().InnerHandler = handler;

            _handlersList.Add(handler);
            return this;
        }

        /// <summary>
        /// Adds <see cref="DelegatingHandler"/> as the last link of the chain.
        /// </summary>
        /// <returns></returns>
        public DelegatingHandler Build()
        {
            if (_handlersList.Any())
                _handlersList.Last().InnerHandler = _rootHandler;
            else
                return _rootHandler;

            return _handlersList.FirstOrDefault();
        }

        /// <summary>
        /// Sets the verbosity for the underlying <see cref="HttpTracerHandler"/>
        /// </summary>
        /// <param name="verbosity"></param>
        /// <returns></returns>
        public HttpHandlerBuilder SetHttpTracerVerbosity(HttpMessageParts verbosity)
        {
            _rootHandler.Verbosity = verbosity;
            return this;
        }

        public HttpHandlerBuilder SetJsonFormatting(JsonFormatting jsonFormatting)
        {
            _rootHandler.JsonFormatting = jsonFormatting;
            return this;
        }
    }
}
