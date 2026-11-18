namespace Nivaes.Serializer.Benchmark.SimplePack;

[SerializeModel]
public class CircularReferencePackerModel
{
    public required string String1 { get; set; }
    public required string String2 { get; set; }

    public CircularRootPackerModel? Reference { get; set; }
}

[SerializeModel]
public class CircularRootPackerModel
{
    public required string String1 { get; set; }
    public required string String2 { get; set; }
    public required CircularReferencePackerModel Reference { get; set; }
}