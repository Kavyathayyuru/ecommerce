namespace ECommerce.Models.DTOs
{
    public class BulkProductImportResultDto
    {
        public bool Success { get; set; }

        public int InsertedCount { get; set; }

        public List<string> Errors { get; set; } = new();
    }
}