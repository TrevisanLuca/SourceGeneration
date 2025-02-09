using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceGeneration.Generator;

[Generator]
public class MetricsDecoratorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            $"{GeneratorHelper.AttributeName}.g.cs",
            SourceText.From(GeneratorHelper.MetricsAttributeDeclaration, Encoding.UTF8)));
    }
}