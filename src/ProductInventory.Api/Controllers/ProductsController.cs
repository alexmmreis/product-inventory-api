using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ProductInventory.Application.Common;
using ProductInventory.Application.Products;
using ProductInventory.Application.Products.Dtos;

namespace ProductInventory.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductResponse>>> GetProducts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductsAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(
        [FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(
        int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productService.UpdateAsync(id, request, cancellationToken);
        return Ok(product);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/decrement-stock/{quantity:int}")]
    public async Task<ActionResult<ProductResponse>> DecrementStock(
        int id, [Range(1, int.MaxValue)] int quantity, CancellationToken cancellationToken)
    {
        var product = await _productService.DecrementStockAsync(id, quantity, cancellationToken);
        return Ok(product);
    }

    [HttpPost("{id:int}/add-to-stock/{quantity:int}")]
    public async Task<ActionResult<ProductResponse>> AddToStock(
        int id, [Range(1, int.MaxValue)] int quantity, CancellationToken cancellationToken)
    {
        var product = await _productService.AddToStockAsync(id, quantity, cancellationToken);
        return Ok(product);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> Search(
        [FromQuery] SearchProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await _productService.SearchAsync(query, cancellationToken);
        return Ok(products);
    }

    [HttpGet("stock-level")]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetByStockRange(
        [FromQuery] StockLevelQuery query, CancellationToken cancellationToken)
    {
        var products = await _productService.GetByStockRangeAsync(query, cancellationToken);
        return Ok(products);
    }
}
