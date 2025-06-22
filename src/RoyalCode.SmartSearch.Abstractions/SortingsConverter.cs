using System.Text.Json.Serialization;
using System.Text.Json;

namespace RoyalCode.SmartSearch.Abstractions;

internal sealed class SortingsConverter : JsonConverter<IReadOnlyList<ISorting>>
{
    public override IReadOnlyList<ISorting>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<List<Sorting>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<ISorting> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}