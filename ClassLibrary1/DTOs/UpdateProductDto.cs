using System;
using System.Collections.Generic;
using System.Text;
namespace ECommerce.Models.DTOs
{
    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}