namespace Nivaes.Serializer.UnitTest;

public class RecursiveCircularReferenceGenerationUnitTest
{
    private readonly ITestOutputHelper _output;

    public RecursiveCircularReferenceGenerationUnitTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [SerializeModel]
    public class ReferenceModel
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }

        public required ReferenceModel Reference1 { get; set; }

        public ReferenceModel? Reference2 { get; set; }
    }

    [Fact]
    public void SerializerVoidDataTest()
    {
        var model = new ReferenceModel()
        {
            String1 = null!,
            String2 = null!,
            Reference1 = new ReferenceModel
            {
                String1 = null!,
                String2 = null!,
                Reference1 = null!
            }
        };
        model.Reference1.Reference1 = model;

        var cache = SerializeModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<ReferenceModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference1.ShouldNotBeNull();
        copyModel.Reference1.String1.ShouldBe(model.Reference1.String1);
        copyModel.Reference1.String2.ShouldBe(model.Reference1.String2);

        copyModel.Reference1.Reference1.ShouldNotBeNull().ShouldBe(copyModel);
        copyModel.Reference1.Reference2.ShouldBeNull();
    }

    [Fact]
    public void SerializerEmptyDataTest()
    {
        var model = new ReferenceModel()
        {
            String1 = string.Empty,
            String2 = string.Empty,
            Reference1 = new ReferenceModel
            {
                String1 = string.Empty,
                String2 = string.Empty,
                Reference1 = null!
            }
        };
        model.Reference1.Reference1 = model;

        var cache = SerializeModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<ReferenceModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference1.ShouldNotBeNull();
        copyModel.Reference1.String1.ShouldBe(model.Reference1.String1);
        copyModel.Reference1.String2.ShouldBe(model.Reference1.String2);

        copyModel.Reference1.Reference1.ShouldNotBeNull().ShouldBe(copyModel);
        copyModel.Reference1.Reference2.ShouldBeNull();
    }

    [Fact]
    public void SerializerTest()
    {
        var model = new ReferenceModel
        {
            String1 = "test1",
            String2 = "test2",
            Reference1 = new ReferenceModel
            {
                String1 = "test3",
                String2 = "test4",
                Reference1 = null!
            }
        };
        model.Reference1.Reference1 = model;

        var cache = SerializeModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        _output.WriteLine(
            string.Join(":", cache.ToArray().Select((b, i) => $"{b}"))
        );


        var copyModel = SerializeModel.Deserialize<ReferenceModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference1.ShouldNotBeNull();
        copyModel.Reference1.String1.ShouldBe(model.Reference1.String1);
        copyModel.Reference1.String2.ShouldBe(model.Reference1.String2);

        copyModel.Reference1.Reference1.ShouldNotBeNull().ShouldBe(copyModel);
        copyModel.Reference1.Reference1.ShouldNotBeNull().ShouldBe(copyModel.Reference1.Reference1);
        copyModel.Reference2.ShouldBeNull();
    }

    [Theory, AutoData]
    public void SerializerAutoDataTest1(string test1, string test2, string test3, string test4)
    {
        var model = new ReferenceModel
        {
            String1 = test1,
            String2 = test2,
            Reference1 = new ReferenceModel
            {
                String1 = test3,
                String2 = test4,
                Reference1 = null!
            }
        };
        model.Reference1.Reference1 = model;

        var cache = SerializeModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<ReferenceModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference1.ShouldNotBeNull();
        copyModel.Reference1.String1.ShouldBe(model.Reference1.String1);
        copyModel.Reference1.String2.ShouldBe(model.Reference1.String2);

        copyModel.Reference1.Reference1.ShouldNotBeNull().ShouldBe(copyModel);
        copyModel.Reference2.ShouldBeNull();
    }
}
