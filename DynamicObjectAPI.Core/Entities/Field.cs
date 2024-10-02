using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicObjectAPI.Domain.Entities
{
    public class Field : BaseEntity
    {
        public int ObjectTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DataType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public ObjectType ObjectType { get; set; } = null!;
    }
}
