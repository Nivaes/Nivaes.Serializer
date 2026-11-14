namespace Nivaes.Serializer.UnitTest;

public class SimplePackUnitTest
{
    private readonly ITestOutputHelper _output;

    public SimplePackUnitTest(ITestOutputHelper output)
    {
        _output = output;
    }

    public class SimpleModel
    {
        public required string String1 { get; init; }
        public required string String2 { get; set; }
    }

    public class SimpleModelTestSerializer : ISerializer<SimpleModel>
    {
        void ISerializer<SimpleModel>.Serialize(scoped in SimpleModel value, ref PackerWriter writer)
        {
            writer.Write(value.String1);
            writer.Write(value.String2);
        }
    }

    public class SimpleModelTestDeserializer : IDeserializer<SimpleModel>
    {
        SimpleModel IDeserializer<SimpleModel>.Deserialize(ref PackerReader reader)
        {
            var string1 = reader.ReadString();
            var string2 = reader.ReadString();

            var value = new SimpleModel
            {
                String1 = string1,
                String2 = string2
            };

            return value;
        }
    }

    [Fact]   
    public void SerializerDataTest()
    {
        var model = new SimpleModel()
        {
            String1 = "String1",
            String2 = "String2",
        };
        
        var cache = SerializeModel.Serialize<SimpleModel, SimpleModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<SimpleModel, SimpleModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
    }


    [Fact]
    public void SerializerVoidDataTest()
    {
        var model = new SimpleModel()
        {
            String1 = null!,
            String2 = null!,
        };

        var cache = SerializeModel.Serialize<SimpleModel, SimpleModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<SimpleModel, SimpleModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
    }

    [Fact]
    public void SerializerEmptyDataTest()
    {
        var model = new SimpleModel()
        {
            String1 = string.Empty,
            String2 = string.Empty,
        };

        var cache = SerializeModel.Serialize<SimpleModel, SimpleModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<SimpleModel, SimpleModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
    }

    [Theory, AutoData]
    public void SerializerAutoDataTest1(string test1, string test2)
    {
        var model = new SimpleModel
        {
            String1 = test1,
            String2 = test2,
        };

        var cache = SerializeModel.Serialize<SimpleModel, SimpleModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializeModel.Deserialize<SimpleModel, SimpleModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
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
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
    }
}
