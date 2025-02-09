namespace SourceGeneration.Program;

public interface IWorker
{
    Task WorkTask();
    Task WorkTaskWithParameters(string myValue);
}

[MetricsDecorator]
public class Worker : IWorker
{
    private readonly Random _rnd = new();

    public async Task WorkTask()
    {
        await Task.Delay(_rnd.Next(100, 1000));
        Console.WriteLine($"{nameof(WorkTask)} done");
    }

    public async Task WorkTaskWithParameters(string myValue)
    {
        await Task.Delay(_rnd.Next(100, 1000));
        Console.WriteLine($"{nameof(WorkTask)} done: {myValue}");
    }
}