using System.Security.Cryptography;

namespace Nivaes.Serializer.UnitTest;

public class MultyArrayTypesPackUnitTest
{
    private readonly ITestOutputHelper _output;

    public MultyArrayTypesPackUnitTest(ITestOutputHelper output)
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
        public required SimpleModel[,] array { get; set; }
    }

    public class CollectionsTypesModelSerializer : ISerializer<CollectionsTypesModel>
    {
        void ISerializer<CollectionsTypesModel>.Serialize(scoped in CollectionsTypesModel value, ref PackerWriter writer)
        {
            writer.WriteArray<SimpleModel, SimpleModelTestSerializer>(value.array);
        }
    }

    public class CollectionsTypesModelDeserializer : IDeserializer<CollectionsTypesModel>
    {
        CollectionsTypesModel IDeserializer<CollectionsTypesModel>.Deserialize(ref PackerReader reader)
        {
            var array1 = (SimpleModel[,])(reader.ReadMultyArray<CollectionsTypesModel, SimpleModel, SimpleModelTestDeserializer>() ?? new SimpleModel[0, 0]);

            var value = new CollectionsTypesModel
            {
                array = array1!   
            };

            return value;
        }
    }

    [Fact]
    public void SerializerTest()
    {
        var model = new CollectionsTypesModel
        {
            array = new[,]
            {
                {
                    new SimpleModel
                    {
                        String1 = "test11-1",
                        String2 = "test11-2"
                    },
                    new SimpleModel
                    {
                        String1 = "test12-1",
                        String2 = "test12-2"
                    },
                    new SimpleModel
                    {
                        String1 = "test13-1",
                        String2 = "test13-2"
                    }
                },
                {
                    new SimpleModel
                    {
                        String1 = "test21-1",
                        String2 = "test21-2"
                    },
                    new SimpleModel
                    {
                        String1 = "test22-1",
                        String2 = "test22-2"
                    },
                    new SimpleModel
                    {
                        String1 = "test23-1",
                        String2 = "test23-2"
                    }
                }
            }
        };

        var cache = SerializePacker.Serialize<CollectionsTypesModel, CollectionsTypesModelSerializer>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializePacker.Deserialize<CollectionsTypesModel, CollectionsTypesModelDeserializer>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.array.ShouldNotBeNull();

        copyModel.array.Rank.ShouldBe(model.array.Rank);

        for (int i = 0; i < model.array.Rank; i++)
        {
            copyModel.array.GetLength(i)
                .ShouldBe(model.array.GetLength(i));
        }

        copyModel.array.ShouldNotBeNull();

        copyModel.array.Length.ShouldBe(model.array.Length);

        foreach (var (item, index) in copyModel.array.Cast<SimpleModel>()
                     .Select((item, index) => (item, index)))
        {
            var original = model.array.Cast<SimpleModel>().ElementAt(index);

            item.String1.ShouldBe(original.String1);
            item.String2.ShouldBe(original.String2);
        }
    }
}
