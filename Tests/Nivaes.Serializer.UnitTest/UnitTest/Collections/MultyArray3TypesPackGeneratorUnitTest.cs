using System.Security.Cryptography;

namespace Nivaes.Serializer.UnitTest;

public class MultyArray3TypesPackGeneratorUnitTest
{
    private readonly ITestOutputHelper _output;

    public MultyArray3TypesPackGeneratorUnitTest(ITestOutputHelper output)
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
        public required SimpleModel[,,] array { get; set; }
    }

    [Fact]
    public void SerializerTest()
    {
        var model = new CollectionsTypesModel
        {
            array = new SimpleModel[,,]
            {
                {
                    {
                        new SimpleModel
                        {
                            String1 = "test111-1",
                            String2 = "test111-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test112-1",
                            String2 = "test112-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test113-1",
                            String2 = "test113-2"
                        }
                    },
                    {
                        new SimpleModel
                        {
                            String1 = "test121-1",
                            String2 = "test121-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test122-1",
                            String2 = "test122-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test123-1",
                            String2 = "test123-2"
                        }
                    }
                },
                {
                    {
                        new SimpleModel
                        {
                            String1 = "test211-1",
                            String2 = "test211-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test212-1",
                            String2 = "test212-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test213-1",
                            String2 = "test213-2"
                        }
                    },
                    {
                        new SimpleModel
                        {
                            String1 = "test221-1",
                            String2 = "test221-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test222-1",
                            String2 = "test222-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test223-1",
                            String2 = "test223-2"
                        }
                    }
                },
                {
                    {
                        new SimpleModel
                        {
                            String1 = "test311-1",
                            String2 = "test311-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test312-1",
                            String2 = "test312-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test313-1",
                            String2 = "test313-2"
                        }
                    },
                    {
                        new SimpleModel
                        {
                            String1 = "test321-1",
                            String2 = "test321-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test322-1",
                            String2 = "test322-2"
                        },
                        new SimpleModel
                        {
                            String1 = "test323-1",
                            String2 = "test323-2"
                        }
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
