using System.Security.Cryptography;

namespace Nivaes.Serializer.UnitTest;

public class ArrayTypesPackUnitTest
{
    private readonly ITestOutputHelper _output;

    public ArrayTypesPackUnitTest(ITestOutputHelper output)
    {
        _output = output;

    }

    public class SimpleModel
    {
        public required string String1 { get; set; }
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

    public class CollectionsTypesModel
    {
        public required SimpleModel[] array { get; set; }
    }

    public class CollectionsTypesModelSerializer : ISerializer<CollectionsTypesModel>
    {
        void ISerializer<CollectionsTypesModel>.Serialize(scoped in CollectionsTypesModel value, ref PackerWriter writer)
        {
            writer.Write<SimpleModel, SimpleModelTestSerializer>(value.array);
        }
    }

    public class CollectionsTypesModelDeserializer : IDeserializer<CollectionsTypesModel>
    {
        private CollectionsTypesModel? _value;

        CollectionsTypesModel IDeserializer<CollectionsTypesModel>.Deserialize(ref PackerReader reader)
        {
            var array1 = reader.ReadArray<CollectionsTypesModel, SimpleModel, SimpleModelTestDeserializer>(Referencearray);

            _value = new CollectionsTypesModel
            {
                array = array1!   
            };

            return _value;
        }

        public void Referencearray(object reference)
        {
            _value!.array = (SimpleModel[])reference;
        }
    }

    [Fact]
    public void SerializerTest()
    {
        var model = new CollectionsTypesModel
        {
            array = new[]
            {
                new SimpleModel
                {
                    String1 = "test1",
                    String2 = "test2"
                },
                new SimpleModel
                {
                    String1 = "test3",
                    String2 = "test4"
                }
            }
        };

        var cache = SerializeModel.Serialize<CollectionsTypesModel, CollectionsTypesModelSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<CollectionsTypesModel, CollectionsTypesModelDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.array.ShouldNotBeNull();
        copyModel.array.Count().ShouldBe(model.array.Count());

        foreach (var (item, index) in copyModel.array.Select((item, index) => (item, index)))
        {
            item.String1.ShouldBe(model.array.ElementAt(index).String1);
            item.String2.ShouldBe(model.array.ElementAt(index).String2);
        }
    }
}
