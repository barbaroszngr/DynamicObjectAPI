using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicObjectAPI.Common.DTOs
{
    public class UpdateObjectRequest
    {
        [Required]
        public string Data { get; set; }
    }
}
