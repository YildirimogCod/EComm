using Microsoft.Extensions.DependencyInjection;

namespace EComm.Ordering;

public static class OrderingModule
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services)
    {
        return services;
    }
}
