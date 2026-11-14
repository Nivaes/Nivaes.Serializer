namespace Nivaes.Serializer.UnitTest;

public class AllTypesPackUnitTest
{
    private readonly ITestOutputHelper _output;

    public AllTypesPackUnitTest(ITestOutputHelper output)
    {
        _output = output;

    }

    public class AllTypesModel
    {
        public required Guid Guid { get; set; }
      
        public required byte Byte1 { get; set; }
        public required byte Byte2 { get; set; }
        public required bool Bool1 { get; set; }
        public required bool Bool2 { get; set; }

        public required byte[] Bytes1 { get; set; }
        public required byte[] Bytes2 { get; set; }

        public required short Short1 { get; set; }
        public required short Short2 { get; set; }
        public required int Int1 { get; set; }
        public required int Int2 { get; set; }
        public required long Long1 { get; set; }
        public required long Long2 { get; set; }

        public required ushort UShort1 { get; set; }
        public required ushort UShort2 { get; set; }
        public required uint UInt1 { get; set; }
        public required uint UInt2 { get; set; }
        public required ulong ULong1 { get; set; }
        public required ulong ULong2 { get; set; }

        public required string String1 { get; set; }
        public required string String2 { get; set; }

        public required float Float1 { get; set; }
        public required float Float2 { get; set; }

        public required double Double1 { get; set; }
        public required double Double2 { get; set; }

        public required Half Half1 { get; set; }
        public required Half Half2 { get; set; }

        public required decimal Decimal1 { get; set; }
        public required decimal Decimal2 { get; set; }

        public required DateTime DateTime1 { get; set; }
        public required DateTime DateTime2 { get; set; }

        public required DateTimeOffset DateTimeOffset1 { get; set; }
        public required DateTimeOffset DateTimeOffset2 { get; set; }
    }
    public class AllTypesModelSerializerTest : ISerializer<AllTypesModel>
    {
        void ISerializer<AllTypesModel>.Serialize(scoped in AllTypesModel item, ref PackerWriter writer)
        {
            writer.Write(item.Guid);

            writer.Write(item.Byte1);
            writer.Write(item.Byte2);

            writer.Write(item.Bytes1);
            writer.Write(item.Bytes2);

            writer.Write(item.Bool1);
            writer.Write(item.Bool2);

            writer.Write(item.Short1);
            writer.Write(item.Short2);
            writer.Write(item.Int1);
            writer.Write(item.Int2);
            writer.Write(item.Long1);
            writer.Write(item.Long2);

            writer.Write(item.UShort1);
            writer.Write(item.UShort2);
            writer.Write(item.UInt1);
            writer.Write(item.UInt2);
            writer.Write(item.ULong1);
            writer.Write(item.ULong2);

            writer.Write(item.String1);
            writer.Write(item.String2);

            writer.Write(item.Float1);
            writer.Write(item.Float2);

            writer.Write(item.Decimal1);
            writer.Write(item.Decimal2);

            writer.Write(item.Double1);
            writer.Write(item.Double2);

            writer.Write(item.Half1);
            writer.Write(item.Half2);

            writer.Write(item.DateTime1);
            writer.Write(item.DateTime2);

            writer.Write(item.DateTimeOffset1);
            writer.Write(item.DateTimeOffset2);
        }
    }

    public class AllTypesModelDeserializerTest : IDeserializer<AllTypesModel>
    {
        AllTypesModel IDeserializer<AllTypesModel>.Deserialize(ref PackerReader reader)
        {
            var value = new AllTypesModel
            {
                Guid = reader.ReadGuid(),

                Byte1 = reader.ReadByte(),
                Byte2 = reader.ReadByte(),

                Bytes1 = reader.ReadBytes()!,
                Bytes2 = reader.ReadBytes()!,

                Bool1 = reader.ReadBoolean(),
                Bool2 = reader.ReadBoolean(),

                Short1 = reader.ReadInt16(),
                Short2 = reader.ReadInt16(),
                Int1 = reader.ReadInt32(),
                Int2 = reader.ReadInt32(),
                Long1 = reader.ReadInt64(),
                Long2 = reader.ReadInt64(),

                UShort1 = reader.ReadUInt16(),
                UShort2 = reader.ReadUInt16(),
                UInt1 = reader.ReadUInt32(),
                UInt2 = reader.ReadUInt32(),
                ULong1 = reader.ReadUInt64(),
                ULong2 = reader.ReadUInt64(),

                String1 = reader.ReadString(),
                String2 = reader.ReadString(),

                Float1 = reader.ReadSingle(),
                Float2 = reader.ReadSingle(),

                Decimal1 = reader.ReadDecimal(),
                Decimal2 = reader.ReadDecimal(),

                Double1 = reader.ReadDouble(),
                Double2 = reader.ReadDouble(),

                Half1 = reader.ReadHalf(),
                Half2 = reader.ReadHalf(),

                DateTime1 = reader.ReadDateTime(),
                DateTime2 = reader.ReadDateTime(),

                DateTimeOffset1 = reader.ReadDateTimeOffset(),
                DateTimeOffset2 = reader.ReadDateTimeOffset()
            };

            return value;
        }
    }

    [Fact]
    public void SerializerTest()
    {
        var model = new AllTypesModel
        {
            Guid = Guid.NewGuid(),

            Byte1 = 223,
            Byte2 = 21,

            Bytes1 = [223, 232, 12, 32, 33, 01, 22, 36, 93],
            Bytes2 = [032, 9, 43, 241, 23, 53, 55],

            Bool1 = true,
            Bool2 = false,

            Short1 = 3844,
            Short2 = -3324,
            Int1 = 233434323,
            Int2 = -233434323,
            Long1 = 30982049302983932,
            Long2 = -219082908320982333,

            UShort1 = 3844,
            UShort2 = 0,
            UInt1 = 233434323,
            UInt2 = 0,
            ULong1 = 30982049302983932,
            ULong2 = 0,

            String1 = "test2",
            String2 = "aklsñdjfñlaskdjfalskd aslkfj a elñkjafkldja ññladsjkf ladjlekjelkjad faldsj lkadje",

            Float1 = 9323.4333292F,
            Float2 = -322.394593393F,

            Decimal1 = 299223.2394323M,
            Decimal2 = -22223.938223M,

            Double1 = 29332.3993949323D,
            Double2 = -23922.3939299294D,

            Half1 = (Half)9323.4333292F,
            Half2 = (Half) (-322.394593393F),

            DateTime1 = DateTime.Now,
            DateTime2 = DateTime.Now.AddSeconds(-2939844),

            DateTimeOffset1 = DateTimeOffset.Now,
            DateTimeOffset2 = DateTimeOffset.Now.AddSeconds(223934),
        };

        var cache = SerializeModel.Serialize<AllTypesModel, AllTypesModelSerializerTest>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
            string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
        );

        var copyModel = SerializeModel.Deserialize<AllTypesModel, AllTypesModelDeserializerTest>(cache);

        copyModel.ShouldNotBeNull();

        copyModel.Byte1.ShouldBe(model.Byte1);
        copyModel.Byte2.ShouldBe(model.Byte2);

        copyModel.Bytes1.ShouldBe(model.Bytes1);
        copyModel.Bytes2.ShouldBe(model.Bytes2);

        copyModel.Bool1.ShouldBe(model.Bool1);
        copyModel.Bool2.ShouldBe(model.Bool2);

        copyModel.Short1.ShouldBe(model.Short1);
        copyModel.Short2.ShouldBe(model.Short2);
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
        copyModel.Long1.ShouldBe(model.Long1);
        copyModel.Long2.ShouldBe(model.Long2);

        copyModel.UShort1.ShouldBe(model.UShort1);
        copyModel.UShort2.ShouldBe(model.UShort2);
        copyModel.UInt1.ShouldBe(model.UInt1);
        copyModel.UInt2.ShouldBe(model.UInt2);
        copyModel.ULong1.ShouldBe(model.ULong1);
        copyModel.ULong2.ShouldBe(model.ULong2);

        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);

        copyModel.Float1.ShouldBe(model.Float1);
        copyModel.Float2.ShouldBe(model.Float2);

        copyModel.Decimal1.ShouldBe(model.Decimal1);
        copyModel.Decimal2.ShouldBe(model.Decimal2);

        copyModel.Double1.ShouldBe(model.Double1);
        copyModel.Double2.ShouldBe(model.Double2);

        copyModel.Half1.ShouldBe(model.Half1);
        copyModel.Half2.ShouldBe(model.Half2);

        copyModel.DateTime1.ShouldBe(model.DateTime1);
        copyModel.DateTime2.ShouldBe(model.DateTime2);

        copyModel.DateTimeOffset1.ShouldBe(model.DateTimeOffset1);
        copyModel.DateTimeOffset2.ShouldBe(model.DateTimeOffset2);
    }

    [Fact]
    public void SerializerTest2()
    {
        var model = new AllTypesModel
        {
            Guid = Guid.Empty,

            Byte1 = Byte.MinValue,
            Byte2 = Byte.MaxValue,

            Bytes1 = [32, 23, 32, 63, 3, 126, 122],
            Bytes2 = [3, 2, 56, 33, 22, 52, 0, 255, 0, 255],

            Bool1 = false,
            Bool2 = true,

            Short1 = short.MaxValue,
            Short2 = short.MinValue,
            Int1 = int.MaxValue,
            Int2 = int.MinValue,
            Long1 = long.MaxValue,
            Long2 = long.MinValue,

            UShort1 = ushort.MaxValue,
            UShort2 = ushort.MinValue,
            UInt1 = uint.MaxValue,
            UInt2 = uint.MinValue,
            ULong1 = ulong.MaxValue,
            ULong2 = ulong.MinValue,

            String1 = "eeedkasñjdlfkajldk alksdjfañs añlsdjkfldkjñ de",
            String2 = "dajsdfñlkjñea alkjañlkejdddas",

            Float1 = float.MaxValue,
            Float2 = float.MinValue,

            Decimal1 = decimal.MinValue,
            Decimal2 = decimal.MaxValue,

            Double1 = double.MinValue,
            Double2 = double.MaxValue,

            Half1 = Half.MaxValue,
            Half2 = Half.MinValue,

            DateTime1 = DateTime.MinValue,
            DateTime2 = DateTime.MaxValue,

            DateTimeOffset1 = DateTimeOffset.MinValue,
            DateTimeOffset2 = DateTimeOffset.MaxValue,
        };

        var cache = SerializeModel.Serialize<AllTypesModel, AllTypesModelSerializerTest>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        _output.WriteLine(
           string.Join(" ", cache.ToArray().Select((b, i) => $"{i}:{b}"))
       );

        var copyModel = SerializeModel.Deserialize<AllTypesModel, AllTypesModelDeserializerTest>(cache);

        copyModel.ShouldNotBeNull();

        copyModel.Byte1.ShouldBe(model.Byte1);
        copyModel.Byte2.ShouldBe(model.Byte2);

        copyModel.Bytes1.ShouldBe(model.Bytes1);
        copyModel.Bytes2.ShouldBe(model.Bytes2);

        copyModel.Bool1.ShouldBe(model.Bool1);
        copyModel.Bool2.ShouldBe(model.Bool2);

        copyModel.Short1.ShouldBe(model.Short1);
        copyModel.Short2.ShouldBe(model.Short2);
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
        copyModel.Long1.ShouldBe(model.Long1);
        copyModel.Long2.ShouldBe(model.Long2);

        copyModel.UShort1.ShouldBe(model.UShort1);
        copyModel.UShort2.ShouldBe(model.UShort2);
        copyModel.UInt1.ShouldBe(model.UInt1);
        copyModel.UInt2.ShouldBe(model.UInt2);
        copyModel.ULong1.ShouldBe(model.ULong1);
        copyModel.ULong2.ShouldBe(model.ULong2);

        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);

        copyModel.Float1.ShouldBe(model.Float1);
        copyModel.Float2.ShouldBe(model.Float2);

        copyModel.Decimal1.ShouldBe(model.Decimal1);
        copyModel.Decimal2.ShouldBe(model.Decimal2);

        copyModel.Half1.ShouldBe(model.Half1);
        copyModel.Half2.ShouldBe(model.Half2);

        copyModel.Double1.ShouldBe(model.Double1);
        copyModel.Double2.ShouldBe(model.Double2);

        copyModel.DateTime1.ShouldBe(model.DateTime1);
        copyModel.DateTime2.ShouldBe(model.DateTime2);

        copyModel.DateTimeOffset1.ShouldBe(model.DateTimeOffset1);
        copyModel.DateTimeOffset2.ShouldBe(model.DateTimeOffset2);
    }

    [Theory, AutoData]
    public void SerializerAllDataTest2(AllTypesModel model)
    {
        var cache = SerializeModel.Serialize<AllTypesModel, AllTypesModelSerializerTest>(model);
        cache.ToArray().ShouldNotBeNull().ShouldNotBeEmpty();

        var copyModel = SerializeModel.Deserialize<AllTypesModel, AllTypesModelDeserializerTest>(cache);

        copyModel.ShouldNotBeNull();
        copyModel.Byte1.ShouldBe(model.Byte1);
        copyModel.Byte2.ShouldBe(model.Byte2);

        copyModel.Bytes1.ShouldBe(model.Bytes1);
        copyModel.Bytes2.ShouldBe(model.Bytes2);

        copyModel.Bool1.ShouldBe(model.Bool1);
        copyModel.Bool2.ShouldBe(model.Bool2);

        copyModel.Short1.ShouldBe(model.Short1);
        copyModel.Short2.ShouldBe(model.Short2);
        copyModel.Int1.ShouldBe(model.Int1);
        copyModel.Int2.ShouldBe(model.Int2);
        copyModel.Long1.ShouldBe(model.Long1);
        copyModel.Long2.ShouldBe(model.Long2);

        copyModel.UShort1.ShouldBe(model.UShort1);
        copyModel.UShort2.ShouldBe(model.UShort2);
        copyModel.UInt1.ShouldBe(model.UInt1);
        copyModel.UInt2.ShouldBe(model.UInt2);
        copyModel.ULong1.ShouldBe(model.ULong1);
        copyModel.ULong2.ShouldBe(model.ULong2);

        copyModel.String1.ShouldBe(model.String1);
        copyModel.String2.ShouldBe(model.String2);

        copyModel.Float1.ShouldBe(model.Float1);
        copyModel.Float2.ShouldBe(model.Float2);

        copyModel.Decimal1.ShouldBe(model.Decimal1);
        copyModel.Decimal2.ShouldBe(model.Decimal2);

        copyModel.Double1.ShouldBe(model.Double1);
        copyModel.Double2.ShouldBe(model.Double2);

        copyModel.Half1.ShouldBe(model.Half1);
        copyModel.Half2.ShouldBe(model.Half2);

        copyModel.DateTime1.ShouldBe(model.DateTime1);
        copyModel.DateTime2.ShouldBe(model.DateTime2);

        copyModel.DateTimeOffset1.ShouldBe(model.DateTimeOffset1);
        copyModel.DateTimeOffset2.ShouldBe(model.DateTimeOffset2);
    }
}
