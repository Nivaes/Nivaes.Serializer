namespace Nivaes.Serializer.UnitTest;

public class SimpleNullPackUnitTest
{
    private readonly ITestOutputHelper _output;

    public SimpleNullPackUnitTest(ITestOutputHelper output)
    {
        _output = output;
    }

    public class SimpleModel
    {
        public required int? Int1 { get; init; }
        public int? Int2 { get; set; }
    }

    public class SimpleModelTestSerializer : ISerializer<SimpleModel>
    {
        void ISerializer<SimpleModel>.Serialize(scoped in SimpleModel value, ref PackerWriter writer)
        {
            writer.Write(value.Int1);
            writer.Write(value.Int2);
        }
    }

    public class SimpleModelTestDeserializer : IDeserializer<SimpleModel>
    {
        SimpleModel IDeserializer<SimpleModel>.Deserialize(ref PackerReader reader)
        {
            var int1 = reader.ReadNullableInt32();
            var int2 = reader.ReadNullableInt32();

            var value = new SimpleModel
            {
                Int1 = int1,
                Int2 = int2     
            };

            return value;
        }
    }

    [Fact]   
    public void SerializerDataTest()
    {
        var model = new SimpleModel()
        {
            Int1 = 342331,
            Int2 = null,
        };
        
        var cache = SerializeModel.Serialize<SimpleModel, SimpleModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<SimpleModel, SimpleModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
    }


    [Fact]
    public void SerializerVoidDataTest()
    {
        var model = new SimpleModel()
        {
            Int1 = null!,
            Int2 = null!,
        };

        var cache = SerializeModel.Serialize<SimpleModel, SimpleModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<SimpleModel, SimpleModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
    }

    [Fact]
    public void SerializerNullDataTest()
    {
        var model = new SimpleModel()
        {
            Int1 = 222,
        };

        var cache = SerializeModel.Serialize<SimpleModel, SimpleModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<SimpleModel, SimpleModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
    }


    [Theory, AutoData]
    public void SerializerAutoDataTest1(int? test1, int? test2)
    {
        var model = new SimpleModel
        {
            Int1 = test1,
            Int2 = test2,
        };

        var cache = SerializeModel.Serialize<SimpleModel, SimpleModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializeModel.Deserialize<SimpleModel, SimpleModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
    }

    [Theory, AutoData]
    public void SerializerAutoDataTest2(SimpleModel model)
    {
        var cache = SerializeModel.Serialize<SimpleModel, SimpleModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<SimpleModel, SimpleModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
    }
}
