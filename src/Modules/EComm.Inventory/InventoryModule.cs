using Microsoft.Extensions.DependencyInjection;

namespace EComm.Inventory;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        return services;
    }
}
