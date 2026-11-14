namespace Nivaes.Serializer.UnitTest;

public class SimpleNullPackGeneratorUnitTest
{
    private readonly ITestOutputHelper _output;

    public SimpleNullPackGeneratorUnitTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [SerializePacker]
    public class SimpleNullModel
    {
        public required int? Int1 { get; init; }
        public int? Int2 { get; set; }
    }

    [Fact]   
    public void SerializerDataTest()
    {
        var model = new SimpleNullModel()
        {
            Int1 = 342331,
            Int2 = null,
        };
        
        var cache = SerializePacker.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializePacker.Deserialize<SimpleNullModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
    }

    [Fact]
    public void SerializerVoidDataTest()
    {
        var model = new SimpleNullModel()
        {
            Int1 = null!,
            Int2 = null!,
        };

        var cache = SerializePacker.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializePacker.Deserialize<SimpleNullModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
    }

    [Fact]
    public void SerializerNullDataTest()
    {
        var model = new SimpleNullModel()
        {
            Int1 = 222,
        };

        var cache = SerializePacker.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializePacker.Deserialize<SimpleNullModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
    }


    [Theory, AutoData]
    public void SerializerAutoDataTest1(int? test1, int? test2)
    {
        var model = new SimpleNullModel
        {
            Int1 = test1,
            Int2 = test2,
        };

        var cache = SerializePacker.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializePacker.Deserialize<SimpleNullModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
    }

    [Theory, AutoData]
    public void SerializerAutoDataTest2(SimpleNullModel model)
    {
        var cache = SerializePacker.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializePacker.Deserialize<SimpleNullModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
    }
}
