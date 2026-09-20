using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantNursery.Application.Common;
using PlantNursery.Application.DTOs.Users;
using PlantNursery.Application.Interfaces;

namespace PlantNursery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // ============================================================
    // Get all users
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();

        return Ok(
            ApiResponse<IEnumerable<UserDto>>.Ok(
                users,
                "Users retrieved successfully."));
    }


    // ============================================================
    // Get user by ID
    // ============================================================

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "User not found."));
        }

        return Ok(
            ApiResponse<UserDto>.Ok(
                user,
                "User retrieved successfully."));
    }


    // ============================================================
    // Create user
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateUserRequest request)
    {
        var user = await _userService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            ApiResponse<UserDto>.Ok(
                user,
                "User created successfully."));
    }


    // ============================================================
    // Update user
    // ============================================================

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateUserRequest request)
    {
        var user = await _userService.UpdateAsync(
            id,
            request);

        if (user == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "User not found."));
        }

        return Ok(
            ApiResponse<UserDto>.Ok(
                user,
                "User updated successfully."));
    }


    // ============================================================
    // Deactivate user
    // ============================================================

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _userService.DeactivateAsync(id);

        if (!result)
        {
            return NotFound(
                ApiResponse.Fail(
                    "User not found."));
        }

        return Ok(
            ApiResponse.Ok(
                "User deactivated successfully."));
    }
}