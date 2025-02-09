namespace SourceGeneration.Generator;

public static class GeneratorHelper
{
    public const string Namespace = "SourceGeneration.Program";
    public const string AttributeName = "MetricsDecoratorAttribute";
    
    public const string MetricsAttributeDeclaration = $"""
    namespace {Namespace};
    
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class {AttributeName} : System.Attribute;
    """;  
}