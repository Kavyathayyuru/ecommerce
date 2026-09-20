using ClosedXML.Excel;
using ECommerce.Data.Repositories;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using System.Globalization;


namespace ECommerce.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<Product>> GetProducts()
        {
            return await _productRepository.GetProducts();
        }

        public async Task<Product?> GetProductById(int id)
        {
            return await _productRepository.GetProductById(id);
        }

        public async Task<Product> AddProduct(CreateProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock
            };

            await _productRepository.AddProduct(product);

            return product;
        }

        public async Task<Product?> UpdateProduct(
            int id,
            UpdateProductDto productDto)
        {
            var existingProduct =
                await _productRepository.GetProductById(id);

            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.Price = productDto.Price;
            existingProduct.Stock = productDto.Stock;

            await _productRepository.UpdateProduct(existingProduct);

            return existingProduct;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var existingProduct =
                await _productRepository.GetProductById(id);

            if (existingProduct == null)
            {
                return false;
            }

            await _productRepository.DeleteProduct(id);

            return true;
        }
        public async Task<bool> DeleteProducts(List<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
                return false;

            await _productRepository.DeleteProducts(productIds);

            return true;
        }
        public async Task<BulkProductImportResultDto> BulkAddProducts(Stream excelStream)
        {
            var result = new BulkProductImportResultDto();

            using var workbook = new XLWorkbook(excelStream);

            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                result.Errors.Add("Excel file does not contain a worksheet.");
                return result;
            }

            var headerRow = worksheet.FirstRowUsed();

            if (headerRow == null)
            {
                result.Errors.Add("Excel file is empty.");
                return result;
            }

            var nameHeader = headerRow.Cell(1).GetString().Trim();
            var descriptionHeader = headerRow.Cell(2).GetString().Trim();
            var priceHeader = headerRow.Cell(3).GetString().Trim();
            var stockHeader = headerRow.Cell(4).GetString().Trim();

            if (nameHeader != "Name" ||
                descriptionHeader != "Description" ||
                priceHeader != "Price" ||
                stockHeader != "Stock")
            {
                result.Errors.Add(
                    "Excel columns must be: Name, Description, Price, Stock.");

                return result;
            }

            var products = new List<Product>();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                if (row.Cells(1, 4).All(cell => cell.IsEmpty()))
                {
                    continue;
                }

                var rowNumber = row.RowNumber();

                var name = row.Cell(1).GetString().Trim();
                var description = row.Cell(2).GetString().Trim();

                var priceText = row.Cell(3).GetString().Trim();
                var stockText = row.Cell(4).GetString().Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    result.Errors.Add(
                        $"Row {rowNumber}: Product name is missing.");
                }

                if (!decimal.TryParse(
                        priceText,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var price))
                {
                    result.Errors.Add(
                        $"Row {rowNumber}: Price must be a valid number.");
                }
                else if (price < 0)
                {
                    result.Errors.Add(
                        $"Row {rowNumber}: Price cannot be negative.");
                }

                if (!int.TryParse(
                        stockText,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var stock))
                {
                    result.Errors.Add(
                        $"Row {rowNumber}: Stock must be a valid whole number.");
                }
                else if (stock < 0)
                {
                    result.Errors.Add(
                        $"Row {rowNumber}: Stock cannot be negative.");
                }

                if (string.IsNullOrWhiteSpace(name) ||
                    !decimal.TryParse(
                        priceText,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out price) ||
                    price < 0 ||
                    !int.TryParse(
                        stockText,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out stock) ||
                    stock < 0)
                {
                    continue;
                }

                products.Add(new Product
                {
                    Name = name,
                    Description = description,
                    Price = price,
                    Stock = stock
                });
            }

            if (result.Errors.Count > 0)
            {
                return result;
            }

            if (products.Count == 0)
            {
                result.Errors.Add("Excel file does not contain any products.");
                return result;
            }

            const int batchSize = 500;

            for (int i = 0; i < products.Count; i += batchSize)
            {
                var batch = products
                    .Skip(i)
                    .Take(batchSize)
                    .ToList();

                await _productRepository.AddProducts(batch);
            }

            result.Success = true;
            result.InsertedCount = products.Count;

            return result;
        }
    }
}