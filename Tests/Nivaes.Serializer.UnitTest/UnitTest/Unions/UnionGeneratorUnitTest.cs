namespace Nivaes.Serializer.UnitTest;

public class UnionGeneratorUnitTest
{
    private readonly ITestOutputHelper _output;

    public UnionGeneratorUnitTest(ITestOutputHelper output)
    {
        _output = output;

    }

    public record class Type1Model
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }
    }

    public record class Type2Model
    {
        public required int Int1 { get; set; }
        public required string String1 { get; set; }
    }

    public record class Type3Model
    {
        public required double Double1 { get; set; }
        public required double Double2 { get; set; }
    }

    [SerializeModel]
    public union UnionModel(Type1Model, Type2Model, Type3Model);

    [Fact]
    public void Type1SerializerTest()
    {
        UnionModel model = new Type1Model()
        {
            String1 = "String1",
            String2 = "String2"
        };

        var cache = SerializerModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
           string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
       );

        var copyModel = SerializerModel.Deserialize<UnionModel>(cache);

        copyModel.Value.ShouldBeOfType<Type1Model>();
        var copyType1 = (Type1Model)copyModel.Value;

        copyType1.String1.ShouldBe(((Type1Model)model.Value).String1);
        copyType1.String2.ShouldBe(((Type1Model)model.Value).String2);
    }

    [Fact]
    public void Type2SerializerTest()
    {
        UnionModel model = new Type2Model()
        {
            Int1 = 42,
            String1 = "String1"
        };

        var cache = SerializerModel.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
           string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
       );

        var copyModel = SerializerModel.Deserialize<UnionModel>(cache);

        //copyModel.ShouldNotBeNull();
        copyModel.Value.ShouldBeOfType<Type2Model>();
        var copyType2 = (Type2Model)copyModel.Value;

        copyType2.Int1.ShouldBe(((Type2Model)model.Value).Int1);
        copyType2.String1.ShouldBe(((Type2Model)model.Value).String1);
    }
}
