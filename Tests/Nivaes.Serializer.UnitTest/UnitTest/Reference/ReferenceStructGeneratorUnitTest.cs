namespace Nivaes.Serializer.UnitTest;

public class ReferenceStructGeneratorUnitTest
{
    private readonly ITestOutputHelper _output;

    public ReferenceStructGeneratorUnitTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [SerializeModel]
    public class RootModel
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }
        public ReferenceStructModel? Reference { get; set; }
    }

    [SerializeModel]
    public struct ReferenceStructModel
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }
    }

    [Fact]
    public void SerializerVoidDataTest()
    {
        var model = new RootModel()
        {
            String1 = null!,
            String2 = null!,
            Reference = new ReferenceStructModel
            {
                String1 = null!,
                String2 = null!,
            }
        };

        var cache = SerializerModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializerModel.Deserialize<RootModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference.ShouldNotBeNull();
        copyModel.Reference.Value.String1.ShouldBe(model.Reference!.Value.String1);
        copyModel.Reference.Value.String2.ShouldBe(model.Reference!.Value.String2);
    }

    [Fact]
    public void SerializerEmptyDataTest()
    {
        var model = new RootModel()
        {
            String1 = string.Empty,
            String2 = string.Empty,
            Reference = new ReferenceStructModel
            {
                String1 = string.Empty,
                String2 = string.Empty,
            }
        };

        var cache = SerializerModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializerModel.Deserialize<RootModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference.ShouldNotBeNull();
        copyModel.Reference.Value.String1.ShouldBe(model.Reference!.Value.String1);
        copyModel.Reference.Value.String2.ShouldBe(model.Reference!.Value.String2);
    }

    [Fact]
    public void SerializerTest()
    {
        var model = new RootModel
        {
            String1 = "test1",
            String2 = "test2",
            Reference = new ReferenceStructModel
            {
                String1 = "test3",
                String2 = "test4",
            }
        };

        var cache = SerializerModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializerModel.Deserialize<RootModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference.ShouldNotBeNull();
        copyModel.Reference.Value.String1.ShouldBe(model.Reference!.Value.String1);
        copyModel.Reference.Value.String2.ShouldBe(model.Reference!.Value.String2);
    }

    [Theory, AutoData]
    public void SerializerAutoDataTest1(string test1, string test2, string test3, string test4)
    {
        var model = new RootModel
        {
            String1 = test1,
            String2 = test2,
            Reference = new ReferenceStructModel
            {
                String1 = test3,
                String2 = test4,
            }
        };

        var cache = SerializerModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializerModel.Deserialize<RootModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference.ShouldNotBeNull();
        copyModel.Reference.Value.String1.ShouldBe(model.Reference!.Value.String1);
        copyModel.Reference.Value.String2.ShouldBe(model.Reference!.Value.String2);
    }
}
