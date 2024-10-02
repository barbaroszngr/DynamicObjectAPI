using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace DynamicObjectAPI.Common.DTOs
{
    public class DynamicUpdateRequest
    {
        [Required(ErrorMessage = "ObjectType is required")]
        public string ObjectType { get; set; }

        [Required(ErrorMessage = "Id is required")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Data is required")]
        public JObject Data { get; set; }
    }
}