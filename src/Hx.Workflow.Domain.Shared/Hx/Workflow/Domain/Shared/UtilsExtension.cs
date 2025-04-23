using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Hx.Workflow.Domain.Shared
{
    public static class UtilsExtension
    {
        public static IDictionary<string, object>? ToDictionaryString(this string json)
        {
            IDictionary<string, object>? value1 = null;
            try
            {
                value1 = JsonSerializer.Deserialize<IDictionary<string, object>>(json);
            }
            catch { }
            return value1;
        }
        public static IDictionary<object, object>? ToDictionaryObject(this string json)
        {
            IDictionary<object, object>? value1 = null;
            try
            {
                value1 = JsonSerializer.Deserialize<IDictionary<object, object>>(json);
            }
            catch { }
            return value1;
        }
    }
}
