using System.Collections.Generic;
using HttpTracer;
using HttpTracer.Logger;

namespace HttpTracer.Tests.Fakes
{
    public class FakeLogger : ILogger
    {
        public readonly List<string> LogHistory = new List<string>();

        public void Log(string message)
        {
            LogHistory.Add(message);
        }
    }
}