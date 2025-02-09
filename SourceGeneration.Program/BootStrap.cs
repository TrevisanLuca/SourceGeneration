using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SourceGeneration.Program;

public static class BootStrap
{
    public static IHost Build()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddSingleton<IWorker, Worker>();
        builder.Services.AddSingleton<IRandomStringGenerator, RandomStringGenerator>();
        builder.Services.AddSingleton<IRunner, Runner>();

        return builder.Build();
    }
}