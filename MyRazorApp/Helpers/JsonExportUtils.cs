using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MyRazorApp.Helpers
{
    public class JsonExportUtils
    {
        private static readonly Lazy<JsonExportUtils> _instance = new(() => new JsonExportUtils());

        public static JsonExportUtils Instance => _instance.Value;

        private JsonExportUtils() { }

        public string SerializeToJson<T>(IEnumerable<T> data)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(data, options);
        }
    }
}
