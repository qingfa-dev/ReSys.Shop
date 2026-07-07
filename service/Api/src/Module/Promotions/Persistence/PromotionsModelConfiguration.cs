using BuildingBlocks.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Module.Promotions.Persistence;

public class PromotionsModelConfiguration : IModuleModelConfiguration
{
    public void Configure(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(PromotionsModelConfiguration).Assembly);
    }
}
