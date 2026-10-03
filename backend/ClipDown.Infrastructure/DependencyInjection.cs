using ClipDown.Application.Common.Interfaces;
using ClipDown.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClipDown.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IImageConverter, ImageSharpImageConverter>();

        return services;
    }
}
