using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using PlantNursery.Application.Common;

namespace PlantNursery.Infrastructure.Identity;

public static class JwtEvents
{
    public static async Task OnChallenge(
        JwtBearerChallengeContext context)
    {
        context.HandleResponse();

        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        context.Response.ContentType = "application/json";

        var response = ApiResponse.Fail(
            "Authentication is required.");

        await context.Response.WriteAsJsonAsync(response);
    }

    public static async Task OnForbidden(
        ForbiddenContext context)
    {
        context.Response.StatusCode =
            StatusCodes.Status403Forbidden;

        context.Response.ContentType = "application/json";

        var response = ApiResponse.Fail(
            "You do not have permission to access this resource.");

        await context.Response.WriteAsJsonAsync(response);
    }
}