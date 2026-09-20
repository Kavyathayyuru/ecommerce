namespace ECommerce.Models.DTOs;

public class BulkDeleteProductsDto
{
    public List<int> ProductIds { get; set; } = new();
}
