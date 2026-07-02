using Module.Profile.Domain.Addresses;

using Shared.Operational.Persistence.Specifications.Querying;

namespace Module.Profile.Features.Store.Addresses.Get.PagedOrAll;

public static partial class GetAddresses
{
    // ============ PARAMETERS ============
    public record Parameters : QueryingParameters
    {
        public AddressType? AddressType { get; init; }
    }
}