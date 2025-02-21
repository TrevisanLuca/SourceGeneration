using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SourceGeneration.Core;
using RegistrationData = (string interfaceName, string className);

namespace SourceGeneration.Generator;

[Generator]
public class MetricsDecoratorGenerator : IIncrementalGenerator
{
    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamespaceSymbol ns)
            {
                foreach (var type in GetAllTypes(ns))
                {
                    yield return type;
                }
            }
            else if (member is INamedTypeSymbol type)
            {
                yield return type;
            }
        }
    }
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //deprecated
        // context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
        //     $"{GeneratorHelper.AttributeName}.g.cs",
        //     SourceText.From(GeneratorHelper.MetricsAttributeDeclaration, Encoding.UTF8)));
        IncrementalValueProvider<Compilation> compilationProvider = context.CompilationProvider;

        // Extract classes with MyAttribute from referenced assemblies
        var classesWithAttribute = compilationProvider.Select((compilation, cancellationToken) =>
        {
            // Look through all referenced assemblies
            var results = new List<(INamedTypeSymbol symbol, AttributeData attribute)>();

            // Include the main compilation assembly and all referenced assemblies
            foreach (var assembly in compilation.Assembly.Modules.SelectMany(m => m.ReferencedAssemblySymbols).Append(compilation.Assembly))
            {
                // Get all types in the assembly
                var types = GetAllTypes(assembly.GlobalNamespace);
                foreach (var type in types)
                {
                    var attribute = type.GetAttributes()
                        .FirstOrDefault(attr => attr.AttributeClass?.Name == typeof(WithMetricsAttribute).Name &&
                                                attr.AttributeClass?.ContainingNamespace.ToString() == typeof(WithMetricsAttribute).Namespace);
                    if (attribute != null)
                    {
                        results.Add((type, attribute));
                    }
                }
            }

            return results;
        });
        
        // var provider = context.CompilationProvider
        //     .ForAttributeWithMetadataName(
        //         typeof(WithMetricsAttribute).FullName!,
        //         predicate: static (_, _) => true,
        //         transform: static (ctx, _) => ctx.TargetNode as ClassDeclarationSyntax)
        //     .Where(static m => m is not null);

        context.RegisterSourceOutput(classesWithAttribute,
            ((ctx, t) => GenerateCode(ctx, t)));
    }

    private static void GenerateCode(SourceProductionContext context, List<(INamedTypeSymbol symbol, AttributeData data)> list)
    {
        var registrationTargets = new List<RegistrationData>();
        // Go through all filtered class declarations.
        foreach (var (symbol, attribute) in list)
        {
            // 'Identifier' means the token of the node. Get class name from the syntax node.
            var className = symbol.Name;
            var decoratorName = $"{className}Decorator";
            const string decorateeName = "decoratee";

            // Go through all class members with a particular type (property) to generate method lines.
            var mainInterface = symbol.Interfaces.First();
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