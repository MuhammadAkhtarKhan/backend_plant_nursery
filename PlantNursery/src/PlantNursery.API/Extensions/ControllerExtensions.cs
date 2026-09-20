using Microsoft.AspNetCore.Mvc;
using PlantNursery.Application.Common;

namespace PlantNursery.API.Extensions;

public static class ControllerExtensions
{
    public static IServiceCollection AddApiControllers(
        this IServiceCollection services)
    {
        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors
                                .Select(e => e.ErrorMessage)
                                .ToArray());

                    return new BadRequestObjectResult(
                        new ApiResponse<object>
                        {
                            Success = false,
                            Message =
                                "One or more validation errors occurred.",
                            Data = errors
                        });
                };
            });

        return services;
    }
}