namespace Nivaes.Serializer.UnitTest;

public class ReferenceUnitTest
{
    private readonly ITestOutputHelper _output;

    public ReferenceUnitTest(ITestOutputHelper output)
    {
        _output = output;
    }

    public class RootModel
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }
        public required ReferenceModel Reference { get; set; }
    }

    public class RootModelTestSerializer : ISerializer<RootModel>
    {
        void ISerializer<RootModel>.Serialize(scoped in RootModel value, ref PackerWriter writer)
        {
            writer.Write(value.String1);
            writer.Write(value.String2);
            writer.Write<ReferenceModel, ReferenceModelTestSerializer>(value.Reference);
        }
    }

    public class RootModelTestDeserializer : IDeserializer<RootModel>
    {
        RootModel IDeserializer<RootModel>.Deserialize(ref PackerReader reader)
        {
            var string1 = reader.ReadString();
            var string2 = reader.ReadString();
            var reference = reader.ReadReference<RootModel, ReferenceModel, ReferenceModelTestDeserializer>()!;

            var value = new RootModel
            {
                String1 = string1,
                String2 = string2,
                Reference = reference
            };
            return value;
        }
    }

    public class ReferenceModel
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }
    }

    public class ReferenceModelTestSerializer : ISerializer<ReferenceModel>
    {
        void ISerializer<ReferenceModel>.Serialize(scoped in ReferenceModel value, ref PackerWriter writer)
        {
            writer.Write(value.String1);
            writer.Write(value.String2);
        }
    }

    public class ReferenceModelTestDeserializer : IDeserializer<ReferenceModel>
    {
        ReferenceModel IDeserializer<ReferenceModel>.Deserialize(ref PackerReader reader)
        {
            var string1 = reader.ReadString();
            var string2 = reader.ReadString();

            var value = new ReferenceModel
            {
                String1 = string1,
                String2 = string2
            };

            return value;
        }
    }

    [Fact]
    public void SerializerVoidDataTest()
    {
        var model = new RootModel()
        {
            String1 = null!,
            String2 = null!,
            Reference = new ReferenceModel
            {
                String1 = null!,
                String2 = null!,
            }
        };

        var cache = SerializeModel.Serialize<RootModel, RootModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<RootModel, RootModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference.ShouldNotBeNull();
        copyModel.Reference.String1.ShouldBe(model.Reference.String1);
        copyModel.Reference.String2.ShouldBe(model.Reference.String2);
    }

    [Fact]
    public void SerializerEmptyDataTest()
    {
        var model = new RootModel()
        {
            String1 = string.Empty,
            String2 = string.Empty,
            Reference = new ReferenceModel
            {
                String1 = string.Empty,
                String2 = string.Empty,
            }
        };

        var cache = SerializeModel.Serialize<RootModel, RootModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<RootModel, RootModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference.ShouldNotBeNull();
        copyModel.Reference.String1.ShouldBe(model.Reference.String1);
        copyModel.Reference.String2.ShouldBe(model.Reference.String2);
    }

    [Fact]
    public void SerializerTest()
    {
        var model = new RootModel
        {
            String1 = "test1",
            String2 = "test2",
            Reference = new ReferenceModel
            {
                String1 = "test3",
                String2 = "test4",
            }
        };

        var cache = SerializeModel.Serialize<RootModel, RootModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<RootModel, RootModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference.ShouldNotBeNull();
        copyModel.Reference.String1.ShouldBe(model.Reference.String1);
        copyModel.Reference.String2.ShouldBe(model.Reference.String2);
    }

    [Theory, AutoData]
    public void SerializerAutoDataTest1(string test1, string test2, string test3, string test4)
    {
        var model = new RootModel
        {
            String1 = test1,
            String2 = test2,
            Reference = new ReferenceModel
            {
                String1 = test3,
                String2 = test4,
            }
        };

        var cache = SerializeModel.Serialize<RootModel, RootModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<RootModel, RootModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference.ShouldNotBeNull();
        copyModel.Reference.String1.ShouldBe(model.Reference.String1);
        copyModel.Reference.String2.ShouldBe(model.Reference.String2);
    }

    [Theory, AutoData]
    public void SerializerAutoDataTest2(RootModel model)
    {
        var cache = SerializeModel.Serialize<RootModel, RootModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<RootModel, RootModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference.ShouldNotBeNull();
        copyModel.Reference.String1.ShouldBe(model.Reference.String1);
        copyModel.Reference.String2.ShouldBe(model.Reference.String2);
    }
}
