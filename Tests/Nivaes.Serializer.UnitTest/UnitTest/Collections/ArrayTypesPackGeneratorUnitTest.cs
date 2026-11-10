//using System.Security.Cryptography;

//namespace Nivaes.Serializer.UnitTest;

//public class ArrayTypesPackGeneratorUnitTest
//{
//    private readonly ITestOutputHelper _output;

//    public ArrayTypesPackGeneratorUnitTest(ITestOutputHelper output)
//    {
//        _output = output;

//    }

//    [SerializePacker]
//    public class SimpleModel
//    {
//        public required string String1 { get; set; }
//        public required string String2 { get; set; }
//    }


//    [SerializePacker]
//    public class CollectionsTypesModel
//    {
//        public required SimpleModel[] array { get; set; }
//    }

//    [Fact]
//    public void SerializerTest()
//    {
//        var model = new CollectionsTypesModel
//        {
//            array = new[]
//            {
//                new SimpleModel
//                {
//                    String1 = "test1",
//                    String2 = "test2"
//                },
//                new SimpleModel
//                {
//                    String1 = "test3",
//                    String2 = "test4"
//                }
//            }
//        };
          
//        var cache = SerializePacker.Serialize(model);
//        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

//        _output.WriteLine(
//            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
//        );

//        var copyModel = SerializePacker.Deserialize<CollectionsTypesModel>(cache);

//        copyModel.ShouldNotBeNull();
//        copyModel.array.ShouldNotBeNull();
//        copyModel.array.Count().ShouldBe(model.array.Count());

//        foreach(var (item, index) in copyModel.array.Select((item, index) => (item, index)))
//        {
//            item.String1.ShouldBe(model.array.ElementAt(index).String1);
//            item.String2.ShouldBe(model.array.ElementAt(index).String2);
//        }
//    }
//}
