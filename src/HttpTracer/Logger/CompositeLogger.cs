using System;
using System.Linq;

namespace HttpTracer.Logger
{
    public class CompositeLogger : ILogger
    {
        private readonly ILogger[] _loggers;

        /// <summary>
        /// Constructs a new <see cref="ILogger"/> that accepts one or more <see cref="ILogger"/>s
        /// </summary>
        /// <param name="loggers">A collection of <see cref="ILogger"/>s to be used when logging <see cref="HttpTracer"/> Trace messages</param>
        /// <exception cref="ArgumentException"><see cref="loggers"/> cannot be null or empty. You must supply one or more</exception>
        public CompositeLogger(params ILogger[] loggers)
        {
            if(loggers == null || !loggers.Any())
                throw new ArgumentException("You must pass at least one logger into the constructor", nameof(loggers));
           
            _loggers = loggers;
        }
        
        /// <summary>
        /// Logs the Trace Message to your specified <see cref="ILogger"/>s
        /// </summary>
        /// <param name="message"><see cref="HttpTracer"/> Trace message</param>
        public void Log(string message)
        {
            foreach (var logger in _loggers)
                logger.Log(message);
        }
    }
}