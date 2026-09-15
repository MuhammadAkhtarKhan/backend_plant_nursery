using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PlantNursery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok(new
        {
            message = "Anyone can access this endpoint."
        });
    }

    [Authorize]
    [HttpGet("authenticated")]
    public IActionResult Authenticated()
    {
        return Ok(new
        {
            message = "You are authenticated.",
            user = User.Identity?.Name
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult Admin()
    {
        return Ok(new
        {
            message = "You are an Admin."
        });
    }

    [Authorize(Roles = "Cashier")]
    [HttpGet("cashier")]
    public IActionResult Cashier()
    {
        return Ok(new
        {
            message = "You are a Cashier."
        });
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("customer")]
    public IActionResult Customer()
    {
        return Ok(new
        {
            message = "You are a Customer."
        });
    }
}