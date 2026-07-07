using BuildingBlocks.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Module.Payment.Persistence;

public class PaymentModelConfiguration : IModuleModelConfiguration
{
    public void Configure(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(PaymentModelConfiguration).Assembly);
    }
}
