
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace TestEfCore.Models
{
    [Table("User")]
    public class User
    {
        public User()
        {
            Products = new HashSet<Product>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Email { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }

    [Table("Product")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedTime { get; set; }

        public int OwnerId { get; set; }

        public virtual User Owner { get; set; }
    }
}
