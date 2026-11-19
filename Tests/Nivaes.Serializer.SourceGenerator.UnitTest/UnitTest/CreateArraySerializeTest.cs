using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Nivaes.Serializer.SourceGenerator.UnitTest;

public class CreateArraySerialize
{
    [Fact]
    public async Task CreateArrayInterfacesTest()
    {
        string source = @"
        using System;
        using Nivaes.Serializer;

        [SerializeModel]
        public class SimpleModel
        {
            public required Guid Guid1 { get; set; }
            public required string String1 { get; set; }
            public required string String2 { get; set; }
        }

        [SerializeModel]
        public class CollectionsTypesModel
        {
            public required SimpleModel[] array { get; set; }
        }
        ";

        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp14);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var compilation = CreateCompilation(syntaxTree);

        IIncrementalGenerator generator = new SerializerPackerGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();

        Assert.Empty(runResult.Diagnostics);
        Assert.NotEmpty(runResult.Results);

        var generatedSources = runResult.Results
            .SelectMany(result => result.GeneratedSources)
            .ToArray();

        var errors = outputCompilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
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
        params SyntaxTree[] syntaxTrees)
    {
        var references = ((string)AppContext.GetData(
            "TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToList();

        references.Add(
            MetadataReference.CreateFromFile(
                typeof(SerializeModelAttribute).Assembly.Location));

        return CSharpCompilation.Create(
            "GeneratorTests",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));
    }
}