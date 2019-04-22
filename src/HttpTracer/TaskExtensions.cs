using System.Threading.Tasks;

namespace HttpTracer
{
    internal static class TaskExtensions
    {
        public static async void FireAndForget(this Task task)
        {
            await task;
        }
    }
}