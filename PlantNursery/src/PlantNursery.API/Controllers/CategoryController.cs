using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantNursery.Application.Common;
using PlantNursery.Application.DTOs.Categories;
using PlantNursery.Application.Interfaces;

namespace PlantNursery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // Admin, Cashier and Customer
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();

        return Ok(
            ApiResponse<IEnumerable<CategoryDto>>.Ok(
                categories,
                "Categories retrieved successfully."));
    }

    // Admin, Cashier and Customer
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "Category not found."));
        }

        return Ok(
            ApiResponse<CategoryDto>.Ok(
                category,
                "Category retrieved successfully."));
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCategoryRequest request)
    {
        var category = await _categoryService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            ApiResponse<CategoryDto>.Ok(
                category,
                "Category created successfully."));
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCategoryRequest request)
    {
        var category = await _categoryService.UpdateAsync(
            id,
            request);

        if (category == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "Category not found."));
        }

        return Ok(
            ApiResponse<CategoryDto>.Ok(
                category,
                "Category updated successfully."));
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _categoryService.DeactivateAsync(id);

        if (!result)
        {
            return NotFound(
                ApiResponse.Fail(
                    "Category not found or already inactive."));
        }

        return Ok(
            ApiResponse.Ok(
                "Category deactivated successfully."));
    }
}