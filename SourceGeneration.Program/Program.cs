using Microsoft.Extensions.DependencyInjection;
using SourceGeneration.Program;

var host = BootStrap.Build();

var runner = host.Services.GetRequiredService<IRunner>();

await runner.Run();