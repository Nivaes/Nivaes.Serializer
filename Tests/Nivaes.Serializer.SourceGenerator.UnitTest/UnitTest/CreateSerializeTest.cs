using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Nivaes.Serializer;

namespace Nivaes.Serializer.SourceGenerator.UnitTest;

public class CreateSerialize
{
    [Fact]
    public void CreateInterfacesTest()
    {
        AssertGeneratorCompiles(
            """
            using System;
            using Nivaes.Serializer;

            [SerializeModel]
            public class SimpleModel
            {
                public required Guid Guid1 { get; set; }
                public required string String1 { get; set; }
                public required string String2 { get; set; }
            }
            """);
    }

    [Fact]
    public void CreateMemoryTypeTest()
    {
        AssertGeneratorCompiles(
            """
            using System;
            using Nivaes.Serializer;

            [SerializeModel]
            public class SimpleModel
            {
                public required Memory<byte> Memory { get; set; }
            }
            """);
    }

    [Fact]
    public void CreateInterfacesNullableTest()
    {
        AssertGeneratorCompiles(
            """
            using System;
            using Nivaes.Serializer;

            [SerializeModel]
            public class SimpleModel
            {
                public required Guid Guid1 { get; set; }
                public required int? Int1 { get; set; }
                public required string String2 { get; set; }
            }
            """);
    }

    private static void AssertGeneratorCompiles(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.CSharp14));

        var compilation = CreateCompilation(syntaxTree);

        IIncrementalGenerator generator = new SerializerPackerGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { generator.AsSourceGenerator() },
            parseOptions: (CSharpParseOptions)syntaxTree.Options);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        var runResult = driver.GetRunResult();

        Assert.Empty(runResult.Diagnostics);
        Assert.Empty(generatorDiagnostics);

        var generatedSources = runResult.Results
            .SelectMany(result => result.GeneratedSources)
            .ToArray();

        Assert.NotEmpty(generatedSources);

        var errors = outputCompilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.Empty(errors);
    }

    private static CSharpCompilation CreateCompilation(SyntaxTree syntaxTree)
    {
        var trustedAssemblies =
            (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        var references = trustedAssemblies
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToList();

        references.Add(
            MetadataReference.CreateFromFile(
                typeof(SerializeModelAttribute).Assembly.Location));

        return CSharpCompilation.Create(
            assemblyName: "GeneratorTests",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));
    }
}