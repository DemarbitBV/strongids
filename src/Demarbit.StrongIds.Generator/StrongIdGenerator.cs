#nullable enable
using System;
using System.Threading;
using Demarbit.StrongIds.Generator.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Demarbit.StrongIds.Generator;

/// <summary>
/// Source code generator for the StrongId struct
/// </summary>
[Generator]
public class StrongIdGenerator : IIncrementalGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structs = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Demarbit.StrongIds.StrongIdAttribute",
                predicate: (node, _) => node is StructDeclarationSyntax,
                transform: ExtractModel)
            .Where(m => m is not null)
            .Select((m, _) => m!);
        
        context.RegisterSourceOutput(structs, (spc, model) =>
        {
            var source = GenerateSource(model);
            spc.AddSource($"{model.FullyQualifiedName}.g.cs", source);
        });
    }

    private static StrongIdModel? ExtractModel(
        GeneratorAttributeSyntaxContext ctx,
        CancellationToken ct)
    {
        var structSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var attribute = ctx.Attributes[0];

        var backingType = BackingType.Guid;
        if (attribute.ConstructorArguments.Length > 0
            && attribute.ConstructorArguments[0].Value is int val)
        {
            backingType = (BackingType)val;
        }

        return new StrongIdModel(
            Namespace: structSymbol.ContainingNamespace.ToDisplayString(),
            Name: structSymbol.Name,
            FullyQualifiedName: structSymbol.ToDisplayString(),
            BackingType: backingType,
            IsPublic: structSymbol.DeclaredAccessibility == Accessibility.Public);
    }

    private static string GenerateSource(StrongIdModel model)
    {
        return model.BackingType switch
        {
            BackingType.Guid => GuidTemplate.Generate(model),
            BackingType.Int => IntTemplate.Generate(model),
            BackingType.Long => LongTemplate.Generate(model),
            _ => StringTemplate.Generate(model)
        };
    }
}

internal record StrongIdModel(
    string Namespace,
    string Name,
    string FullyQualifiedName,
    BackingType BackingType,
    bool IsPublic);