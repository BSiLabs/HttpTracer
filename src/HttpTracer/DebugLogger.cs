using System;
using System.Diagnostics;

namespace HttpTracer
{
    public class DebugLogger : ILogger
    {
        public void Log(string message) => Debug.WriteLine(message);
    }
}
