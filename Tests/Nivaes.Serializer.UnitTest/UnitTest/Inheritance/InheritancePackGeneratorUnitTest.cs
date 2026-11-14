namespace Nivaes.Serializer.UnitTest;

public class InheritancePackGeneratorUnitTest
{
    private readonly ITestOutputHelper _output;

    public InheritancePackGeneratorUnitTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [SerializeModel]
    public class SimpleModel:BaseModel
    {
        public required int Int1 { get; init; }
        public required string? String2 { get; set; }
    }

    public class BaseModel
    {
        public required string String1 { get; init; }
        public required uint? Int2 { get; set; }

        [SerializeIgnore]
        public double? Double1 { get; set; }
    }
   

    [Fact]   
    public void SerializerDataTest()
    {
        var model = new SimpleModel()
        {
            String1 = "String1",
            String2 = "String2",
            Int1 = 122332,
            Int2 = 33332,
            Double1 = 2.33222
        };
        
        var cache = SerializerModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializerModel.Deserialize<SimpleModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
        copyModel.Double1.ShouldBeNull();
        copyModel.Double1.ShouldNotBe(model.Double1);
    }


    [Fact]
    public void SerializerVoidDataTest()
    {
        var model = new SimpleModel()
        {
            String1 = null!,
            String2 = null!,
            Int1 = 0,
            Int2 = null,
            Double1 = 2
        };

        var cache = SerializerModel.Serialize<SimpleModel>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializerModel.Deserialize<SimpleModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
        copyModel.Double1.ShouldBeNull();
        copyModel.Double1.ShouldNotBe(model.Double1);
    }

    [Fact]
    public void SerializerEmptyDataTest()
    {
        var model = new SimpleModel()
        {
            String1 = string.Empty,
            String2 = string.Empty,
            Int1 = 0,
            Int2 = null,
            Double1 = 0
        };

        var cache = SerializerModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializerModel.Deserialize<SimpleModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
        copyModel.Double1.ShouldBeNull();
        copyModel.Double1.ShouldNotBe(model.Double1);
    }

    [Theory, AutoData]
    public void SerializerAutoDataTest1(string test1, string test2, int test3, uint? test4, double double1)
    {
        var model = new SimpleModel
        {
            String1 = test1,
            String2 = test2,
            Int1 = test3,
            Int2 = test4,
            Double1 = double1
        };

        var cache = SerializerModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializerModel.Deserialize<SimpleModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
        copyModel.Double1.ShouldBeNull();
        copyModel.Double1.ShouldNotBe(model.Double1);
    }

    [Theory, AutoData]
    public void SerializerAutoDataTest2(SimpleModel model)
    {
        var cache = SerializerModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializerModel.Deserialize<SimpleModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
        copyModel.Double1.ShouldBeNull();
        copyModel.Double1.ShouldNotBe(model.Double1);
    }
}
