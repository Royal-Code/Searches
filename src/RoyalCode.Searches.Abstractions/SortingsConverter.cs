using System.Text.Json.Serialization;
using System.Text.Json;

namespace RoyalCode.Searches.Abstractions;

internal class SortingsConverter : JsonConverter<IEnumerable<ISorting>>
{
    public override IEnumerable<ISorting>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<List<Sorting>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<ISorting> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}