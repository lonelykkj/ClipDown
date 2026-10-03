using ClipDown.Application.Common.Interfaces;
using ClipDown.Application.Features.Images;
using Microsoft.Extensions.DependencyInjection;

namespace ClipDown.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IImageConversionService, ImageConversionService>();

        return services;
    }
}
