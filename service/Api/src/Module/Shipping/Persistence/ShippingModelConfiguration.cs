using BuildingBlocks.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Module.Shipping.Persistence;

public class ShippingModelConfiguration : IModuleModelConfiguration
{
    public void Configure(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ShippingModelConfiguration).Assembly);
    }
}
