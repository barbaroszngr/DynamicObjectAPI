using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace DynamicObjectAPI.Common.DTOs
{
    public class DynamicGetAllRequest
    {
        [Required(ErrorMessage = "ObjectType is required")]
        public string ObjectType { get; set; }

        public JObject Filters { get; set; }
    }
}