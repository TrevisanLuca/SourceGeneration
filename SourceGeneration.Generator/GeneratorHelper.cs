namespace SourceGeneration.Generator;

public static class GeneratorHelper
{
    public const string Namespace = "SourceGeneration.Program";
    public const string AttributeName = "MetricsDecoratorAttribute";
    
    public const string MetricsAttributeDeclaration = $"""
    using System;
    
    namespace {Namespace};
    
    [AttributeUsage(AttributeTargets.Class)]
    public class {AttributeName} : Attribute;
    """;  
}