using System;

namespace HttpTracer.Logger
{
    public class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Logs the Trace Message to your Console
        /// </summary>
        /// <param name="message"><see cref="HttpTracer"/> Trace message</param>
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}