using Microsoft.EntityFrameworkCore;

namespace Shared.Operational.Persistence.Configurations.Numbers;

public static class DecimalConfiguration
{
    #region Conventions
    public static void ConfigureConvention(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>()
            .HavePrecision(NumberConstant.Constraints.DecimalPrecision, NumberConstant.Constraints.DecimalScale);

        configurationBuilder.Properties<decimal?>()
             .HavePrecision(NumberConstant.Constraints.DecimalPrecision, NumberConstant.Constraints.DecimalScale);
    }
    #endregion
}