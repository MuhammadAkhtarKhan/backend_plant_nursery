using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantNursery.Application.Common;
using PlantNursery.Application.DTOs.Plants;
using PlantNursery.Application.Interfaces;

namespace PlantNursery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlantController : ControllerBase
{
    private readonly IPlantService _plantService;

    public PlantController(IPlantService plantService)
    {
        _plantService = plantService;
    }

    // Admin, Cashier and Customer
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var plants = await _plantService.GetAllAsync();

        return Ok(
            ApiResponse<IEnumerable<PlantDto>>.Ok(
                plants,
                "Plants retrieved successfully."));
    }

    // Admin, Cashier and Customer
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var plant = await _plantService.GetByIdAsync(id);

        if (plant == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "Plant not found."));
        }

        return Ok(
            ApiResponse<PlantDto>.Ok(
                plant,
                "Plant retrieved successfully."));
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePlantRequest request)
    {
        var plant = await _plantService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = plant.Id },
            ApiResponse<PlantDto>.Ok(
                plant,
                "Plant created successfully."));
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdatePlantRequest request)
    {
        var plant = await _plantService.UpdateAsync(
            id,
            request);

        if (plant == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "Plant not found."));
        }

        return Ok(
            ApiResponse<PlantDto>.Ok(
                plant,
                "Plant updated successfully."));
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _plantService.DeactivateAsync(id);

        if (!result)
        {
            return NotFound(
                ApiResponse.Fail(
                    "Plant not found or already inactive."));
        }

        return Ok(
            ApiResponse.Ok(
                "Plant deactivated successfully."));
    }
}