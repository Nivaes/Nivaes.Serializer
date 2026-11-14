
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Nivaes.Serializer.SourceGenerator.UnitTest;

public class Interceptor
{
    [Fact]
    public void InterceptorTest()
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

        public class Program
        {
            public static void Main(string[] args)
            {
                var model = new SimpleModel
                {
                    Guid1 = Guid.NewGuid(),
                    String1 = ""Hello"",
                    String2 = ""World""
                };

                var cache = SerializerModel.Serialize(model);
                var copyModel = SerializerModel.Deserialize<SimpleModel>(cache);
            }
        }
        ";

        // Opciones de C# e Interceptors.
        var parseOptions = new CSharpParseOptions(
            LanguageVersion.CSharp14)
            .WithFeatures(
            [
                new KeyValuePair<string, string>(
                    "InterceptorsNamespaces",
                    "Nivaes.Serializer.SourceGenerator")
            ]);

        // Crear SyntaxTree con las opciones configuradas.
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            parseOptions);

        var compilation = CreateCompilation(
            syntaxTree,
            parseOptions);

        // Ejecutar ambos generadores.
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new ISourceGenerator[]
            {
                new SerializerPackerGenerator().AsSourceGenerator(),
                new SerializerInterceptorGenerator().AsSourceGenerator()
            },
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        // Comprobar errores de los generadores.
        var runResult = driver.GetRunResult();

        Assert.Empty(runResult.Diagnostics);

        Assert.NotEmpty(runResult.Results);

        // Mostrar código generado.
        var generatedSources = runResult.Results
            .SelectMany(x => x.GeneratedSources)
            .ToArray();

        foreach (var generated in generatedSources)
        {
            Console.WriteLine($"===== {generated.HintName} =====");
            Console.WriteLine(generated.SourceText.ToString());
        }

        // Comprobar errores de compilación.
        var errors = outputCompilation.GetDiagnostics()
            .Where(x => x.Severity == DiagnosticSeverity.Error)
            .ToArray();

        errors.ShouldBeEmpty();
    }

    private static CSharpCompilation CreateCompilation(
        SyntaxTree syntaxTree,
        CSharpParseOptions parseOptions)
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
            [syntaxTree],
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));
    }
}
