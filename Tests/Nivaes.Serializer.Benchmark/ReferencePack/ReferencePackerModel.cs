namespace Nivaes.Serializer.Benchmark.SimplePack;

[SerializeModel]
public class ReferencePackerModel
{
    public required string String1 { get; set; }
    public required string String2 { get; set; }
}

[SerializeModel]
public class RootPackerModel
{
    public required string String1 { get; set; }
    public required string String2 { get; set; }
    public required ReferencePackerModel Reference { get; set; }
}