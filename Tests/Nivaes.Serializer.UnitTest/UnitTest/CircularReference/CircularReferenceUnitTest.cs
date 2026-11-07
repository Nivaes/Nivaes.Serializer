namespace Nivaes.Serializer.UnitTest;

public class CircularReferenceUnitTest
{
    private readonly ITestOutputHelper _output;

    public CircularReferenceUnitTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [SerializePacker]
    public class RootModel
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }

        public required ReferenceModel Reference1 { get; set; }
        public required ReferenceModel Reference2 { get; set; }
    }

    public class RootModelTestSerializer : ISerializer<RootModel>
    {
        void ISerializer<RootModel>.Serialize(scoped in RootModel model, ref PackerWriter writer)
        {
            writer.Write(model.String1);
            writer.Write(model.String2);
            writer.Write<ReferenceModel, ReferenceModelSerializer>(model.Reference1);
            writer.Write<ReferenceModel, ReferenceModelSerializer>(model.Reference2);
        }
    }

    public class RootModelTestDeserializer : IDeserializer<RootModel>
    {
        private RootModel? _value;

        RootModel IDeserializer<RootModel>.Deserialize(ref PackerReader reader)
        {
            var string1 = reader.ReadString();
            var string2 = reader.ReadString();
            var reference1 = reader.ReadReference<RootModel, ReferenceModel, ReferenceModelDeserializerTest>(ReferenceReference1!);
            var reference2 = reader.ReadReference<RootModel, ReferenceModel, ReferenceModelDeserializerTest>(ReferenceReference2!);

            _value = new RootModel
            {
                String1 = string1,
                String2 = string2,
                Reference1 = reference1!,
                Reference2 = reference2!

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

    [SerializePacker]
    public class ReferenceModel
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }

        public RootModel? RootReference { get; set; }
    }

    public class ReferenceModelSerializer : ISerializer<ReferenceModel>
    {
        //int ISerializer.IdClassType => 20;

        void ISerializer<ReferenceModel>.Serialize(scoped in ReferenceModel model, ref PackerWriter writer)
        {
            writer.Write(model.String1);
            writer.Write(model.String2);
            writer.Write<RootModel, RootModelTestSerializer>(model.RootReference);
        }
    }

    public class ReferenceModelDeserializerTest : IDeserializer<ReferenceModel>
    {
        ReferenceModel? _value;

        ReferenceModel IDeserializer<ReferenceModel>.Deserialize(ref PackerReader reader)
        {
            var string1 = reader.ReadString();
            var string2 = reader.ReadString();
            var rootReference = reader.ReadReference<ReferenceModel, RootModel, RootModelTestDeserializer>(ReferenceReference1!);

            _value = new ReferenceModel
            {
                String1 = string1,
                String2 = string2,
                RootReference = rootReference
            };

            return _value;
        }

        public void ReferenceReference1(object reference)
        {
            _value!.RootReference = (RootModel)reference;
        }
    }

    [Fact]
    public void SerializerVoidDataTest()
    {
        var model = new RootModel()
        {
            String1 = null!,
            String2 = null!,
            Reference1 = new ReferenceModel
            {
                String1 = null!,
                String2 = null!,
            },
            Reference2 = new ReferenceModel
            {
                String1 = null!,
                String2 = null!,
            }
        };
        model.Reference1.RootReference = model;
        model.Reference2.RootReference = model;

        var cache = SerializePacker.Serialize<RootModel, RootModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializePacker.Deserialize<RootModel, RootModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference1.ShouldNotBeNull();
        copyModel.Reference1.String1.ShouldBe(model.Reference1.String1);
        copyModel.Reference1.String2.ShouldBe(model.Reference1.String2);

        copyModel.Reference1.RootReference.ShouldNotBeNull();
        copyModel.Reference1.RootReference.ShouldBe(copyModel);
    }

    [Fact]
    public void SerializerEmptyDataTest()
    {
        var model = new RootModel()
        {
            String1 = string.Empty,
            String2 = string.Empty,
            Reference1 = new ReferenceModel
            {
                String1 = string.Empty,
                String2 = string.Empty,
            },
            Reference2 = new ReferenceModel
            {
                String1 = string.Empty,
                String2 = string.Empty,
            },
        };
        model.Reference1.RootReference = model;
        model.Reference2.RootReference = model;

        var cache = SerializePacker.Serialize<RootModel, RootModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializePacker.Deserialize<RootModel, RootModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference1.ShouldNotBeNull();
        copyModel.Reference1.String1.ShouldBe(model.Reference1.String1);
        copyModel.Reference1.String2.ShouldBe(model.Reference1.String2);

        copyModel.Reference1.RootReference.ShouldNotBeNull();
        copyModel.Reference1.RootReference.ShouldBe(copyModel);

        copyModel.Reference2.RootReference.ShouldNotBeNull();
        copyModel.Reference2.RootReference.ShouldBe(copyModel);

    }

    [Fact]
    public void SerializerTest()
    {
        var model = new RootModel
        {
            String1 = "test1",
            String2 = "test2",
            Reference1 = new ReferenceModel
            {
                String1 = "test3",
                String2 = "test4",
            },
            Reference2 = new ReferenceModel
            {
                String1 = "test5",
                String2 = "test6",
            }
        };
        model.Reference1.RootReference = model;
        model.Reference2.RootReference = model;

        var cache = SerializePacker.Serialize<RootModel, RootModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializePacker.Deserialize<RootModel, RootModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference1.ShouldNotBeNull();
        copyModel.Reference1.String1.ShouldBe(model.Reference1.String1);
        copyModel.Reference1.String2.ShouldBe(model.Reference1.String2);

        copyModel.Reference1.RootReference.ShouldNotBeNull();
        copyModel.Reference1.RootReference.ShouldBe(copyModel);

        copyModel.Reference2.RootReference.ShouldNotBeNull();
        copyModel.Reference2.RootReference.ShouldBe(copyModel);
    }

    [Theory, AutoData]
    public void SerializerAutoDataTest1(string test1, string test2, string test3, string test4, string test5, string test6)
    {
        var model = new RootModel
        {
            String1 = test1,
            String2 = test2,
            Reference1 = new ReferenceModel
            {
                String1 = test3,
                String2 = test4,
            },
            Reference2 = new ReferenceModel
            {
                String1 = test5,
                String2 = test6,
            }
        };
        model.Reference1.RootReference = model;
        model.Reference2.RootReference = model;

        var cache = SerializePacker.Serialize<RootModel, RootModelTestSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializePacker.Deserialize<RootModel, RootModelTestDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);
        copyModel.Reference1.ShouldNotBeNull();
        copyModel.Reference1.String1.ShouldBe(model.Reference1.String1);
        copyModel.Reference1.String2.ShouldBe(model.Reference1.String2);

        copyModel.Reference1.RootReference.ShouldNotBeNull();
        copyModel.Reference1.RootReference.ShouldBe(copyModel);

        copyModel.Reference2.RootReference.ShouldNotBeNull();
        copyModel.Reference2.RootReference.ShouldBe(copyModel);

    }
}
