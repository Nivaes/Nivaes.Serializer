namespace Nivaes.Serializer.UnitTest;

public class RecursiveCircularReferenceUnitTest
{
    private readonly ITestOutputHelper _output;

    public RecursiveCircularReferenceUnitTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [SerializePacker]
    public class ReferenceModel
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }

        public required ReferenceModel Reference1 { get; set; }

        public ReferenceModel? Reference2 { get; set; }
    }

    public class ReferenceModelTestSerializer : ISerializer<ReferenceModel>
    {
        void ISerializer<ReferenceModel>.Serialize(scoped in ReferenceModel model, ref PackerWriter writer)
        {
            writer.Write(model.String1);
            writer.Write(model.String2);
            writer.Write<ReferenceModel, ReferenceModelTestSerializer>(model.Reference1);
            writer.Write<ReferenceModel, ReferenceModelTestSerializer>(model.Reference2);
        }
    }

    public class ReferenceModelTestDeserializer : IDeserializer<ReferenceModel>
    {
        private ReferenceModel? _value;
        ReferenceModel IDeserializer<ReferenceModel>.Deserialize(ref PackerReader reader)
        {
            var string1 = reader.ReadString();
            var string2 = reader.ReadString();
            var reference1 = reader.ReadReference<ReferenceModel, ReferenceModel, ReferenceModelTestDeserializer>(ReferenceReference1!);
            var reference2 = reader.ReadReference<ReferenceModel, ReferenceModel, ReferenceModelTestDeserializer>(ReferenceReference2!);

            _value = new ReferenceModel
            {
                String1 = string1,
                String2 = string2,
                Reference1 = reference1!,
                Reference2 = reference2
            };

            return _value;
        }
   
        public void ReferenceReference1(object reference)
        {
            _value!.Reference1 = (ReferenceModel)reference;
        }

        public void ReferenceReference2(object reference)
        {
            _value!.Reference2 = (ReferenceModel)reference;
        }
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

        var cache = SerializePacker.Serialize<ReferenceModel, ReferenceModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializePacker.Deserialize<ReferenceModel, ReferenceModelTestDeserializer>(cache);

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

        var cache = SerializePacker.Serialize<ReferenceModel, ReferenceModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializePacker.Deserialize<ReferenceModel, ReferenceModelTestDeserializer>(cache);

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

        var cache = SerializePacker.Serialize<ReferenceModel, ReferenceModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        _output.WriteLine(
            string.Join(":", cache.ToArray().Select((b, i) => $"{b}"))
        );


        var copyModel = SerializePacker.Deserialize<ReferenceModel, ReferenceModelTestDeserializer>(cache);

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

        var cache = SerializePacker.Serialize<ReferenceModel, ReferenceModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializePacker.Deserialize<ReferenceModel, ReferenceModelTestDeserializer>(cache);

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
