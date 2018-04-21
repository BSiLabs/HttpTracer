using System.Threading.Tasks;

namespace HttpTracer
{
    internal static class TaskExtensions
    {
        public static void FireAndForget(this Task task)
        {
            Task.Run(() => task);
        }
    }
}