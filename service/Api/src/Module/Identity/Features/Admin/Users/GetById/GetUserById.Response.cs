using Module.Identity.Features.Admin.Users.Shared.Models;

namespace Module.Identity.Features.Admin.Users.GetById;

public static partial class GetUserById
{
    public record CustomerGroupDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Presentation { get; init; }
    }

    public record Response : UserDetailResponse
    {
        public IEnumerable<CustomerGroupDto> CustomerGroups { get; set; } = [];
    }
}