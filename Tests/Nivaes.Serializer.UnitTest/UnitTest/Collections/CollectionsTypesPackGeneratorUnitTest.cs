using System.Security.Cryptography;

namespace Nivaes.Serializer.UnitTest;

public class CollectionsTypesPackGeneratorUnitTest
{
    private readonly ITestOutputHelper _output;

    public CollectionsTypesPackGeneratorUnitTest(ITestOutputHelper output)
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
        public required IEnumerable<SimpleModel> Enumerable { get; set; }
    }

    [Fact]
    public void SerializerTest()
    {
        var model = new CollectionsTypesModel
        {
            Enumerable = new[]
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
          
        var cache = SerializeModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<CollectionsTypesModel>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Enumerable.ShouldNotBeNull();
        copyModel.Enumerable.Count().ShouldBe(model.Enumerable.Count());

        foreach(var (item, index) in copyModel.Enumerable.Select((item, index) => (item, index)))
        {
            item.String1.ShouldBe(model.Enumerable.ElementAt(index).String1);
            item.String2.ShouldBe(model.Enumerable.ElementAt(index).String2);
        }
    }
}
