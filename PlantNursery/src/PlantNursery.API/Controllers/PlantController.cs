using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        return Ok(plants);
    }

    // Admin, Cashier and Customer
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var plant = await _plantService.GetByIdAsync(id);

        if (plant == null)
        {
            return NotFound(new
            {
                message = "Plant not found."
            });
        }

        return Ok(plant);
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePlantRequest request)
    {
        try
        {
            var plant = await _plantService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = plant.Id },
                plant);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdatePlantRequest request)
    {
        try
        {
            var plant = await _plantService.UpdateAsync(
                id,
                request);

            if (plant == null)
            {
                return NotFound(new
                {
                    message = "Plant not found."
                });
            }

            return Ok(plant);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // Admin only
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _plantService.DeactivateAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                message = "Plant not found or already inactive."
            });
        }

        return Ok(new
        {
            message = "Plant deactivated successfully."
        });
    }
}