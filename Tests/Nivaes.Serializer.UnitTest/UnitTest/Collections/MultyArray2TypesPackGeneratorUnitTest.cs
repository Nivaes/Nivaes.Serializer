using System.Security.Cryptography;

namespace Nivaes.Serializer.UnitTest;

public class MultyArray2TypesPackGeneratorUnitTest
{
    private readonly ITestOutputHelper _output;

    public MultyArray2TypesPackGeneratorUnitTest(ITestOutputHelper output)
    {
        _output = output;

    }

    [SerializeModel]
    public class SimpleModel
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }
    }


    [SerializeModel]
    public class CollectionsTypesModel
    {
        public required SimpleModel[,] array { get; set; }
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

        var cache = SerializeModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<CollectionsTypesModel>(cache);

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
