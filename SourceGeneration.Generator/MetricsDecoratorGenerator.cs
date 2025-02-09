using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RegistrationData = (string interfaceName, string className);

namespace SourceGeneration.Generator;

[Generator]
public class MetricsDecoratorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            $"{GeneratorHelper.AttributeName}.g.cs",
            SourceText.From(GeneratorHelper.MetricsAttributeDeclaration, Encoding.UTF8)));

        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"{GeneratorHelper.Namespace}.{GeneratorHelper.AttributeName}",
                predicate: static (s, _) => true,
                transform: static (ctx, _) => ctx.TargetNode as ClassDeclarationSyntax)
            .Where(static m => m is not null);

        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
            ((ctx, t) => GenerateCode(ctx, t.Left, t.Right!)));
    }

    private void GenerateCode(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classDeclarations)
    {
        var registrationTargets = new List<RegistrationData>();
        // Go through all filtered class declarations.
        foreach (var classDeclarationSyntax in classDeclarations)
        {
            // We need to get semantic model of the class to retrieve metadata.
            var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            // Symbols allow us to get the compile-time information.
            if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                continue;

            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            // 'Identifier' means the token of the node. Get class name from the syntax node.
            var className = classDeclarationSyntax.Identifier.Text;
            var decoratorName = $"{className}Decorator";
            const string decorateeName = "decoratee";

            // Go through all class members with a particular type (property) to generate method lines.
            var mainInterface = classSymbol.Interfaces.First();
            var interfaceName = mainInterface.ToDisplayString();
            var methods = mainInterface.GetMembers().OfType<IMethodSymbol>()
                .Select(m => $"""
                                  public {m.ReturnType.ToDisplayString()} {m.Name}({string.Join(", ", m.Parameters.Select(p => $@"{p.Type.ToDisplayString()} {p.Name}"))})
                                      => MetricsHandler.DoWithMetrics("loggerParameters", "{m.Name}", () => {decorateeName}.{m.Name}({string.Join(", ", m.Parameters.Select(p => p.Name))}));
                              """);
            
            // Build up the source code
            var code = $$"""
                        using MetricsLogger;
                        
                        namespace {{GeneratorHelper.Namespace}};
                        
                        public class {{decoratorName}}({{interfaceName}} {{decorateeName}}) : {{interfaceName}}
                        {             
                        {{string.Join("\n\n", methods)}}
                        }                         
                        """;


            // Add the source code to the compilation.
            context.AddSource($"{decoratorName}.g.cs", SourceText.From(code, Encoding.UTF8));
            registrationTargets.Add((interfaceName, decoratorName));
        }


        const string services = "services";
        const string decoratorsRegistrationClass = "RegisterMetricsDecorators"; 
        var decoratorsRegistration = registrationTargets.Select(t => $@"{services}.Decorate<{t.interfaceName}, {t.className}>();");

        var registrationCode = $$"""
                               using Microsoft.Extensions.DependencyInjection;
                               using Scrutor;
                               
                               namespace {{GeneratorHelper.Namespace}};
                               
                               public static partial class BootStrap
                               { 
                                    public static void {{decoratorsRegistrationClass}}(IServiceCollection {{services}})
                                    {
                                        {{string.Join("\n", decoratorsRegistration)}}
                                    }
                               }
                               """;
        context.AddSource($"{decoratorsRegistrationClass}.g.cs", SourceText.From(registrationCode, Encoding.UTF8));
    }
}