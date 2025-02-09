namespace SourceGeneration.Program;

public interface IRunner
{
    Task Run();
}

public class Runner(IWorker worker, IRandomStringGenerator stringGen) : IRunner
{
    public async Task Run()
    {
        while (true)
        {
            await Task.WhenAll(
                worker.WorkTask(),
                worker.WorkTaskWithParameters(stringGen.Next()));
        }
    }
}