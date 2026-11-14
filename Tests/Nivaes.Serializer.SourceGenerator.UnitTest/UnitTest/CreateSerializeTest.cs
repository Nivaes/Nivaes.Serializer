using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Nivaes.Serializer.SourceGenerator.UnitTest;

public class CreateSerialize
{
    [Fact]
    public async Task CreateInterfacesTest()
    {
        string source = @"
        using System;
        using Nivaes.Serializer;

        [SerializePacker]
        public class SimpleModel 
        {
            public required Guid Guid1 { get; set; }
            public required string String1 { get; set; }
            public required string String2 { get; set; }
        }
        ";

        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.CSharp14));

        var compilation = CreateCompilation(syntaxTree);

        IIncrementalGenerator generator = new SerializerPackerGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generator.AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        // Assert
        var runResult = driver.GetRunResult();

        Assert.Empty(runResult.Diagnostics);

        Assert.NotEmpty(runResult.Results);

        var generatedSources = runResult.Results
            .SelectMany(x => x.GeneratedSources)
            .ToArray();

        // Comprobar que el código generado compila.
        var errors = outputCompilation.GetDiagnostics()
            .Where(x => x.Severity == DiagnosticSeverity.Error)
            .ToArray();

        errors.ShouldBeEmpty();

        foreach (var result in runResult.Results)
        {
            foreach (var generated in result.GeneratedSources)
            {
                Console.WriteLine($"===== {generated.HintName} =====");
                Console.WriteLine(generated.SourceText.ToString());
            }
        }
    }

    [Fact]
    public async Task CreateMemoryTypeTest()
    {
        string source = @"
        using System;
        using Nivaes.Serializer;

        [SerializePacker]
        public class SimpleModel 
        {
            public required Memory<byte> Memory { get; set; }
        }
        ";

        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.CSharp14));

        var compilation = CreateCompilation(syntaxTree);

        IIncrementalGenerator generator = new SerializerPackerGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generator.AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        // Assert
        var runResult = driver.GetRunResult();

        Assert.Empty(runResult.Diagnostics);

        Assert.NotEmpty(runResult.Results);

        var generatedSources = runResult.Results
            .SelectMany(x => x.GeneratedSources)
            .ToArray();

        // Comprobar que el código generado compila.
        var errors = outputCompilation.GetDiagnostics()
            .Where(x => x.Severity == DiagnosticSeverity.Error)
            .ToArray();

        errors.ShouldBeEmpty();

        foreach (var result in runResult.Results)
        {
            foreach (var generated in result.GeneratedSources)
            {
                Console.WriteLine($"===== {generated.HintName} =====");
                Console.WriteLine(generated.SourceText.ToString());
            }
        }
    }

    [Fact]
    public async Task CreateInterfacesNullableTest()
    {
        string source = @"
        using System;
        using Nivaes.Serializer;

        [SerializePacker]
        public class SimpleModel 
        {
            public required Guid Guid1 { get; set; }
            public required int? Int1 { get; set; }
            public required string String2 { get; set; }
        }
        ";

        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.CSharp14));

        var compilation = CreateCompilation(syntaxTree);

        IIncrementalGenerator generator = new SerializerPackerGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generator.AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        // Assert
        var runResult = driver.GetRunResult();

        Assert.Empty(runResult.Diagnostics);

        Assert.NotEmpty(runResult.Results);

        var generatedSources = runResult.Results
            .SelectMany(x => x.GeneratedSources)
            .ToArray();

        // Comprobar que el código generado compila.
        var errors = outputCompilation.GetDiagnostics()
            .Where(x => x.Severity == DiagnosticSeverity.Error)
            .ToArray();

        errors.ShouldBeEmpty();

        foreach (var result in runResult.Results)
        {
            foreach (var generated in result.GeneratedSources)
            {
                Console.WriteLine($"===== {generated.HintName} =====");
                Console.WriteLine(generated.SourceText.ToString());
            }
        }
    }

    private static CSharpCompilation CreateCompilation(
        SyntaxTree syntaxTree)
    {
        var references = ((string)AppContext.GetData(
            "TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToList();

        references.Add(
            MetadataReference.CreateFromFile(
                typeof(SerializePackerAttribute).Assembly.Location));

        return CSharpCompilation.Create(
            "GeneratorTests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));
    }
}
