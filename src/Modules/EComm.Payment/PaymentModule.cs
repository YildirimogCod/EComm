using Microsoft.Extensions.DependencyInjection;

namespace EComm.Payment;

public static class PaymentModule
{
    public static IServiceCollection AddPaymentModule(this  IServiceCollection services)
    {
        return services;
    }
}
