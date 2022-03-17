using Blauhaus.Common.ValueObjects.Base;
using System.Text.Json.Serialization;

namespace Blauhaus.Orleans.Abstractions.Identity;

public class GrainId : BaseValueObject<GrainId>
{

    [JsonConstructor]
    public GrainId(string interfaceTypeName, string id)
    {
        InterfaceTypeName = interfaceTypeName;
        Id = id;
    }

    public string InterfaceTypeName { get; }
    public string Id { get; }

    protected override int GetHashCodeCore()
    {
        return InterfaceTypeName.GetHashCode() ^ Id.GetHashCode();
    }

    protected override bool EqualsCore(GrainId other)
    {
        return other.Id == Id && other.InterfaceTypeName == InterfaceTypeName;
    }

    public override string ToString()
    {
        var shortName = InterfaceTypeName.Substring(InterfaceTypeName.LastIndexOf('.') + 1);
        return $"{shortName} (Id {Id})";
    }
}