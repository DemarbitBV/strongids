using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Demarbit.StrongIds.Generator.Tests;

public class GeneratorTests
{
    [Fact]
    public Task GuidStrongId_GeneratesExpectedOutput()
    {
        var source = """
                     using Demarbit.StrongIds;
                     
                     namespace TestNamespace;
                     
                     [StrongId]
                     public partial struct OrderId {}
                     """;

        var generator = new StrongIdGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var compilation = CreateCompilation(source);
        
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        return Verify(driver);
    }
    
    [Fact]
    public Task StringStrongId_GeneratesExpectedOutput()
    {
        var source = """
                     using Demarbit.StrongIds;

                     namespace TestNamespace;

                     [StrongId(BackingType.String)]
                     public partial struct OrderId {}
                     """;

        var generator = new StrongIdGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var compilation = CreateCompilation(source);
        
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        return Verify(driver);
    }
    
    
    
    [Fact]
    public Task LongStrongId_GeneratesExpectedOutput()
    {
        var source = """
                     using Demarbit.StrongIds;

                     namespace TestNamespace;

                     [StrongId(BackingType.Long)]
                     public partial struct OrderId {}
                     """;

        var generator = new StrongIdGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var compilation = CreateCompilation(source);
        
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        return Verify(driver);
    }
    
    
    
    [Fact]
    public Task IntStrongId_GeneratesExpectedOutput()
    {
        var source = """
                     using Demarbit.StrongIds;

                     namespace TestNamespace;

                     [StrongId(BackingType.Int)]
                     public partial struct OrderId {}
                     """;

        var generator = new StrongIdGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var compilation = CreateCompilation(source);
        
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        return Verify(driver);
    }
    
    private static CSharpCompilation CreateCompilation(string source)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .Append(MetadataReference.CreateFromFile(typeof(StrongIdAttribute).Assembly.Location))
            .ToList();

        return CSharpCompilation.Create("TestAssembly",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}