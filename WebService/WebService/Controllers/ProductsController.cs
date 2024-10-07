﻿using Microsoft.AspNetCore.Mvc;
using WebService.Interfaces;
using WebService.Models;

namespace WebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService) =>
            _productService = productService;

        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProduct()
        {
            var products = await _productService.GetProduct();

            if (products == null || products.Count == 0)
            {
                return NotFound(new { Message = "No products found." });
            }

            return Ok(products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.Description,
                p.Stock,
                p.LowStockLvl,
                StockStatus = p.StockStatus, // Include stock status in response
                p.Image,
                p.IsActive,
                p.Product_idVendor,
                p.ProductVendorName,
                p.ProductListName
            }));
        }

        // Get product by ID with stock status
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(string id)
        {
            var product = await _productService.GetProductById(id);

            if (product is null)
            {
                return NotFound();
            }

            return Ok(new
            {
                product.Id,
                product.Name,
                product.Price,
                product.Description,
                product.Stock,
                product.LowStockLvl,
                StockStatus = product.StockStatus, // Include stock status in response
                product.Image,
                product.IsActive,
                product.ProductListName
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post(Product newProduct)
        {
            await _productService.CreateProduct(newProduct);
            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, Product updatedProduct)
        {
            var product = await _productService.GetProductById(id);

            if (product is null)
            {
                return NotFound();
            }

            updatedProduct.Id = product.Id;

            await _productService.UpdateProduct(id, updatedProduct);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _productService.GetProductById(id);

            if (product is null)
            {
                return NotFound();
            }

            await _productService.RemoveProduct(id);

            return Ok(new { Message = "Product deleted successfully" });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(string id)
        {
            var product = await _productService.GetProductById(id);

            if (product is null)
            {
                return NotFound();
            }

            await _productService.ChangeProductStatus(id);

            return NoContent();
        }

        [HttpPut("stocks/update/{id}")]
        public async Task<IActionResult> UpdateStock(string id, [FromBody] StockUpdate stockUpdate)
        {
            var product = await _productService.GetProductById(id);

            if (product is null)
            {
                return NotFound();
            }

            if (stockUpdate.Type == 1)
            {
                // Add stock
                await _productService.UpdateStock(id, stockUpdate.StockChange);
            }
            else if (stockUpdate.Type == 0)
            {
                // Reduce stock
                await _productService.UpdateStock(id, -stockUpdate.StockChange);
            }

            return NoContent();
        }

        [HttpPut("stocks/reset/{id}")]
        public async Task<IActionResult> ResetStock(string id)
        {
            var product = await _productService.GetProductById(id);

            if (product is null)
            {
                return NotFound();
            }

            await _productService.ResetStock(id);
            return NoContent();
        }
    }
}
