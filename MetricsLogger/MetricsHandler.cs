using System.Diagnostics;

namespace MetricsLogger;

public static class MetricsHandler
{
    public static async Task DoWithMetrics(string logValues, string methodDescription, Func<Task> func)
    {
        var timer = new Stopwatch();
        timer.Start();
        await func();
        timer.Stop();
        await Console.Out.WriteLineAsync($"{logValues}; {methodDescription}; Elapsed: {timer.ElapsedMilliseconds} ms.");
    }
    
    public static async Task<T> DoWithMetrics<T>(string logValues, string methodDescription, Func<Task<T>> func)
    {
        var timer = new Stopwatch();
        timer.Start();
        var result = await func();
        timer.Stop();
        await Console.Out.WriteLineAsync($"{logValues}; {methodDescription}; Elapsed: {timer.ElapsedMilliseconds} ms.");
        return result;
    }
}