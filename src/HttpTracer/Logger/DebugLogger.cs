using System.Diagnostics;

namespace HttpTracer.Logger
{
    public class DebugLogger : ILogger
    {
        /// <summary>
        /// Logs the Trace Message to your Debug Window
        /// </summary>
        /// <param name="message"><see cref="HttpTracer"/> Trace message</param>
        public void Log(string message) => Debug.WriteLine(message);
    }
}
