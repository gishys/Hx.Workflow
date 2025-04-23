using Newtonsoft.Json;

namespace Hx.Workflow.Domain
{
    public static class JsonExtensions
    {
        public static T? SafeDeserialize<T>(this string? json, JsonSerializerSettings? options = null)
            where T : class, new()
        {
            return string.IsNullOrEmpty(json)
                ? new T() // 或返回 null（需 T 允许为 null）
                : JsonConvert.DeserializeObject<T>(json, options);
        }
    }
}
