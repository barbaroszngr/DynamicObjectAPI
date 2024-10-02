using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicObjectAPI.Domain.Entities
{
    public class ObjectType : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public bool IsMaster { get; set; }
        public int? ParentObjectTypeId { get; set; }
        public ObjectType? ParentObjectType { get; set; }

        public ICollection<Field> Fields { get; set; } = new List<Field>();
        public ICollection<ObjectType> ChildObjectTypes { get; set; } = new List<ObjectType>();
    }
}
