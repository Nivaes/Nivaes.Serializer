namespace Nivaes.Serializer.UnitTest;

public class UnionUnitTest
{
    private readonly ITestOutputHelper _output;

    public UnionUnitTest(ITestOutputHelper output)
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

    public union UnionModel(Type1Model, Type2Model, Type3Model);

    public class UnionModelSerializerTest : ISerializer<UnionModel>
    {
        void ISerializer<UnionModel>.Serialize(scoped in UnionModel item, ref PackerWriter writer)
        {
            switch (item)
            {
                case Type1Model type1:
                    writer.Write(1);
                    writer.Write(type1.String1);
                    writer.Write(type1.String2);
                    break;

                case Type2Model type2:
                    writer.Write(2);
                    writer.Write(type2.Int1);
                    writer.Write(type2.String1);
                    break;

                case Type3Model type3:
                    writer.Write(3);
                    writer.Write(type3.Double1);
                    writer.Write(type3.Double2);
                    break;
            }
        }
    }

    public class UnionModelDeserializerTest : IDeserializer<UnionModel>
    {
        UnionModel IDeserializer<UnionModel>.Deserialize(ref PackerReader reader)
        {
            int idType = reader.ReadInt32();
            if(idType == 1)
            {
                var string1 = reader.ReadString();
                var string2 = reader.ReadString();

                var value = new Type1Model
                {
                    String1 = string1,
                    String2 = string2
                };
                return value;
            }
            else if (idType == 2)
            {
                var int1 = reader.ReadInt32();
                var string1 = reader.ReadString();

                var value = new Type2Model
                {
                    Int1 = int1,
                    String1 = string1,
                };
                return value;
            }
            else if (idType == 3)
            {
                var double1 = reader.ReadDouble();
                var double2 = reader.ReadDouble();

                var value = new Type3Model
                {
                    Double1 = double1,
                    Double2 = double2
                };
                return value;
            }

            throw new Exception("Invalid id type to deserialize UnionModel");
        }
    }

    [Fact]
    public void Type1SerializerTest()
    {
        UnionModel model = new Type1Model()
        {
            String1 = "String1",
            String2 = "String2"
        };

        var cache = SerializerModel.Serialize<UnionModel, UnionModelSerializerTest>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
           string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
       );

        var copyModel = SerializerModel.Deserialize<UnionModel, UnionModelDeserializerTest>(cache);

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

        var cache = SerializerModel.Serialize<UnionModel, UnionModelSerializerTest>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
           string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
       );

        var copyModel = SerializerModel.Deserialize<UnionModel, UnionModelDeserializerTest>(cache);

        //copyModel.ShouldNotBeNull();
        copyModel.Value.ShouldBeOfType<Type2Model>();
        var copyType2 = (Type2Model)copyModel.Value;

        copyType2.Int1.ShouldBe(((Type2Model)model.Value).Int1);
        copyType2.String1.ShouldBe(((Type2Model)model.Value).String1);
    }
}
