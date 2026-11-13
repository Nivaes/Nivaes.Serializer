using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MemoryPack;

namespace Nivaes.Serializer.Benchmark.SimplePack;

//[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
//[SimpleJob(RunStrategy.Throughput)]
//[SimpleJob(launchCount: 1, warmupCount: 0, iterationCount: 1)]
public class PackBenchmarks
{
    //[Params(100, 1_000, 10_000, 100_000, 1_000_000)]
    [Params(100, 1_000)]
    public int Length;

    private string _text1 = null!;
    private string _text2 = null!;
    private string _text3 = null!;
    private string _text4 = null!;

    private byte[]? _nivaesPackSerialize;
    private byte[]? _memoryPackSerialize;
    private byte[]? _nivaesPackReferenceSerialize;
    private byte[]? _memoryPackReferenceSerialize;

    private byte[]? _nivaesPackCircularReferenceSerialize;
    //private byte[]? _memoryPackCircularReferenceSerialize;

    [GlobalSetup]
    public void Setup()
    {
        _text1 = TextHelper.Create(Length);
        _text2 = TextHelper.Create(Length);
        _text3 = TextHelper.Create(Length);
        _text4 = TextHelper.Create(Length);

        _nivaesPackSerialize = PrepareNivaesSerialize();
        _memoryPackSerialize = PrepareMemoryPackSerialize();

        _nivaesPackReferenceSerialize = PrepareNivaesReferenceSerialize();
        _memoryPackReferenceSerialize = PrepareReferenceMemoryPackSerialize();

        _nivaesPackCircularReferenceSerialize = PrepareNivaesCircularReferenceSerialize();
        //_memoryPackCircularReferenceSerialize = PrepareCircularReferenceMemoryPackSerialize();
    }

    #region Serializer
    private byte[] PrepareNivaesSerialize()
    {
        var model = new SimpleNivaesSerializeModel
        {
            String1 = _text1,
            String2 = _text2,
            Int1 = 42
        };

        return SerializePacker.Serialize(model).ToArray();
    }

    [Benchmark]
    public byte[]? NivaesSerialize()
    {
        //Debugger.Launch();

        return PrepareNivaesSerialize();
    }

    [Benchmark]
    public SimpleNivaesSerializeModel? NivaesDeserialize()
    {
        return SerializePacker.Deserialize<SimpleNivaesSerializeModel>(_nivaesPackSerialize);
    }

    private byte[] PrepareMemoryPackSerialize()
    {
        var model = new SimpleMemoryPackModel
        {
            String1 = _text1,
            String2 = _text2,
            Int1 = 42
        };

        return MemoryPackSerializer.Serialize(model);
    }

    [Benchmark]
    public byte[]? MemoryPackSerialize()
    {
        return PrepareMemoryPackSerialize();
    }

    [Benchmark]
    public SimpleMemoryPackModel? MemoryPackDeserialize()
    {
        return MemoryPackSerializer.Deserialize<SimpleMemoryPackModel>(_memoryPackSerialize);
    }
    #endregion

    #region Reference
    private byte[] PrepareNivaesReferenceSerialize()
    {
        var model = new RootPackerModel
        {
            String1 = _text1,
            String2 = _text2,

            Reference = new ReferencePackerModel
            {
                String1 = _text3,
                String2 = _text4,
            }
        };

        return SerializePacker.Serialize(model).ToArray();
    }

    [Benchmark]
    public byte[]? NivaesReferenceSerialize()
    {
        return PrepareNivaesReferenceSerialize();
    }

    [Benchmark]
    public RootPackerModel? NivaesReferenceDeserialize()
    {
        return SerializePacker.Deserialize<RootPackerModel>(_nivaesPackReferenceSerialize);
    }

    private byte[] PrepareReferenceMemoryPackSerialize()
    {
        var model = new RootMemoryPackModel
        {
            String1 = _text1,
            String2 = _text2,
            Reference = new ReferenceMemoryPackModel
            {
                String1 = _text3,
                String2 = _text4,
            }
        };

        return MemoryPackSerializer.Serialize(model);
    }

    [Benchmark]
    public byte[]? MemoryPackReferenceSerialize()
    {
        return PrepareReferenceMemoryPackSerialize();
    }

    [Benchmark]
    public RootMemoryPackModel? MemoryPackReferenceDeserialize()
    {
        return MemoryPackSerializer.Deserialize<RootMemoryPackModel>(_memoryPackReferenceSerialize);
    }
    #endregion

    #region Circular Reference
    private byte[] PrepareNivaesCircularReferenceSerialize()
    {
        var model = new CircularRootPackerModel
        {
            String1 = _text1,
            String2 = _text2,

            Reference = new CircularReferencePackerModel
            {
                String1 = _text3,
                String2 = _text4,
            }
        };

        model.Reference.Reference = model;

        return SerializePacker.Serialize(model).ToArray();
    }

    [Benchmark]
    public byte[]? NivaesCircularReferenceSerialize()
    {
        return PrepareNivaesCircularReferenceSerialize();
    }

    [Benchmark]
    public CircularRootPackerModel? NivaesCircularReferenceDeserialize()
    {
        return SerializePacker.Deserialize<CircularRootPackerModel>(_nivaesPackCircularReferenceSerialize);
    }

    //private byte[] PrepareCircularReferenceMemoryPackSerialize()
    //{
    //    var model = new CircularRootMemoryPackModel
    //    {
    //        String1 = _text1,
    //        String2 = _text2,
    //        Reference = new CircularReferenceMemoryPackModel
    //        {
    //            String1 = _text3,
    //            String2 = _text4,
    //        }
    //    };

    //    model.Reference.Reference = model;

    //    return MemoryPackSerializer.Serialize(model);
    //}

    //[Benchmark]
    //public byte[]? MemoryPackCircularReferenceSerialize()
    //{
    //    return PrepareCircularReferenceMemoryPackSerialize();
    //}

    //[Benchmark]
    //public CircularRootMemoryPackModel? MemoryPackCircularReferenceDeserialize()
    //{
    //    return MemoryPackSerializer.Deserialize<CircularRootMemoryPackModel>(_memoryPackCircularReferenceSerialize);
    //}
    #endregion
}
