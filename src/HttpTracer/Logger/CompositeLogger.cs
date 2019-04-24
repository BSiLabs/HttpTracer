using System;
using System.Linq;

namespace HttpTracer.Logger
{
    public class CompositeLogger : ILogger
    {
        private readonly ILogger[] _loggers;

        public CompositeLogger(params ILogger[] loggers)
        {
            if(loggers == null || !loggers.Any())
                throw new ArgumentException("You must pass at least one logger into the constructor", nameof(loggers));
           
            _loggers = loggers;
        }
        
        public void Log(string message)
        {
            foreach (var logger in _loggers)
                logger.Log(message);
        }
    }
}