using System.Diagnostics;

namespace HttpTracer.Logger
{
    public class DebugLogger : ILogger
    {
        public void Log(string message) => Debug.WriteLine(message);
    }
}
