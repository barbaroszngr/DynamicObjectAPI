using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DynamicObjectAPI.Common.DTOs
{
    public class DynamicListResponse
    {
        public IEnumerable<JObject> Data { get; set; } = new List<JObject>();
    }
}