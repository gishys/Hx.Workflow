using Hx.Workflow.Domain.Persistence;
using System;
using System.Security.Cryptography;
using System.Text.Json;
using System.IO;
using System.Linq;

namespace Hx.Workflow.Application
{
    internal static class WkActivitySubmissionPolicies
    {
        public static string ComputeRequestHash(string payload)
        {
            using var document = JsonDocument.Parse(payload);
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                WriteCanonical(writer, document.RootElement);
            }
            return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
        }

        public static bool ShouldPublishEvent(WkEvent? existingEvent)
            => existingEvent == null;

        private static void WriteCanonical(Utf8JsonWriter writer, JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var property in element.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal))
                    {
                        writer.WritePropertyName(property.Name);
                        WriteCanonical(writer, property.Value);
                    }
                    writer.WriteEndObject();
                    break;
                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                    {
                        WriteCanonical(writer, item);
                    }
                    writer.WriteEndArray();
                    break;
                case JsonValueKind.String:
                    writer.WriteStringValue(element.GetString());
                    break;
                case JsonValueKind.Number:
                    writer.WriteRawValue(element.GetRawText());
                    break;
                case JsonValueKind.True:
                    writer.WriteBooleanValue(true);
                    break;
                case JsonValueKind.False:
                    writer.WriteBooleanValue(false);
                    break;
                default:
                    writer.WriteNullValue();
                    break;
            }
        }
    }
}
