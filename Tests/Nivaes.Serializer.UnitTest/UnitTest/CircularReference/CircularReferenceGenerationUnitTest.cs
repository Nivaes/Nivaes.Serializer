namespace Nivaes.Serializer.UnitTest;

public class CircularReferenceGenerationUnitTest
{
    private readonly ITestOutputHelper _output;

    public CircularReferenceGenerationUnitTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [SerializePacker]
    public class RootModel
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }

        public required ReferenceModel Reference1 { get; init; }
        public required ReferenceModel Reference2 { get; set; }
    }

    [SerializePacker]
    public class ReferenceModel
    {
        public required string String1 { get; set; }
        public required string String2 { get; set; }

        public RootModel? RootReference { get; set; }
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

        var cache = SerializePacker.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializePacker.Deserialize<RootModel>(cache);

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

        var cache = SerializePacker.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializePacker.Deserialize<RootModel>(cache);

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

        var cache = SerializePacker.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializePacker.Deserialize<RootModel>(cache);

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

        var cache = SerializePacker.Serialize(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializePacker.Deserialize<RootModel>(cache);

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
