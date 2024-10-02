using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynamicObjectAPI.Common.DTOs
{
    public class DynamicCreateRequest
    {
        [Required(ErrorMessage = "ObjectType is required")]
        [JsonProperty("objectType")]
        public string ObjectType { get; set; }

        [Required(ErrorMessage = "Data is required")]
        [JsonProperty("data")]
        public JObject Data { get; set; }
    }
}