using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicObjectAPI.Domain.Entities
{
    public class Order : BaseEntity
    {
       
        public DateTime? OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        [Required]
        public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }

}
