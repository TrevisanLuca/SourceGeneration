using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SourceGeneration.Core;

namespace SourceGeneration.Program;

public static partial class BootStrap
{
    public static IHost Build()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddSingleton<IWorker, Worker>();
        builder.Services.AddSingleton<IRandomStringGenerator, RandomStringGenerator>();
        builder.Services.AddSingleton<IRunner, Runner>();

        RegisterMetricsDecorators(builder.Services);
        
        return builder.Build();
    }
}