using Shared.Operational.Persistence.Specifications.Paging;
using Shared.Operational.Persistence.Specifications.Querying;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Querying;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Querying")]
public sealed class QueryingParametersExtensionsTests
{
    [Fact(DisplayName = "ParseAll: Empty parameters returns empty QueryingModel")]
    public void ParseAll_EmptyParameters_ShouldReturnEmptyModel()
    {
        QueryingParameters parameters = QueryingParameters.Empty;

        Result<QueryingModel> result = parameters.ParseAll();

        result.IsSuccess.Should().BeTrue();
        result.Value.Filter.IsEmpty.Should().BeTrue();
        result.Value.Search.IsEmpty.Should().BeTrue();
        result.Value.Sort.IsEmpty.Should().BeTrue();
        result.Value.Page.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "ParseAll: Valid filter string produces FilterModel")]
    public void ParseAll_ValidFilter_ShouldParseFilter()
    {
        QueryingParameters parameters = QueryingParameters.Empty with { Filter = "Name=test" };

        Result<QueryingModel> result = parameters.ParseAll();

        result.IsSuccess.Should().BeTrue();
        result.Value.Filter.IsEmpty.Should().BeFalse();
        result.Value.Filter.Conditions.Should().HaveCount(1);
    }

    [Fact(DisplayName = "ParseAll: Valid search produces SearchingModel")]
    public void ParseAll_ValidSearch_ShouldParseSearch()
    {
        QueryingParameters parameters = QueryingParameters.Empty with { Search = "hello" };

        Result<QueryingModel> result = parameters.ParseAll();

        result.IsSuccess.Should().BeTrue();
        result.Value.Search.IsEmpty.Should().BeFalse();
        result.Value.Search.Term.Value.Should().Be("hello");
    }

    [Fact(DisplayName = "ParseAll: Valid sort produces SortModel")]
    public void ParseAll_ValidSort_ShouldParseSort()
    {
        QueryingParameters parameters = QueryingParameters.Empty with { Sort = ["Name:asc"] };

        Result<QueryingModel> result = parameters.ParseAll();

        result.IsSuccess.Should().BeTrue();
        result.Value.Sort.IsEmpty.Should().BeFalse();
        result.Value.Sort.Clauses.Should().HaveCount(1);
        result.Value.Sort.Clauses[0].Field.Should().Be("Name");
    }

    [Fact(DisplayName = "ParseAll: Valid page produces PageModel")]
    public void ParseAll_ValidPage_ShouldParsePage()
    {
        QueryingParameters parameters = QueryingParameters.Empty with { PageNumber = 2, PageSize = 25 };

        Result<QueryingModel> result = parameters.ParseAll();

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Page.Should().Be(2);
        result.Value.Page.PageSize.Should().Be(25);
    }

    [Fact(DisplayName = "ParseAll: Allowed fields enforce whitelists")]
    public void ParseAll_AllowedFields_ShouldEnforceWhitelists()
    {
        HashSet<string> filterFields = new(StringComparer.OrdinalIgnoreCase) { "Name" };
        HashSet<string> sortFields = new(StringComparer.OrdinalIgnoreCase) { "Name" };

        QueryingParameters parameters = QueryingParameters.Empty with
        {
            Filter = "Disallowed=value",
            Sort = ["Disallowed:asc"]
        };

        Result<QueryingModel> result = parameters.ParseAll(
            allowedFilterFields: filterFields,
            allowedSortFields: sortFields);

        result.IsSuccess.Should().BeTrue();
        result.Value.Filter.IsValid.Should().BeFalse();
        result.Value.Sort.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "ParseAll: All four concerns together")]
    public void ParseAll_AllConcerns_ShouldParseAll()
    {
        QueryingParameters parameters = QueryingParameters.Empty with
        {
            Filter = "Name=test",
            Search = "hello",
            Sort = ["Name:asc"],
            PageNumber = 1,
            PageSize = 10
        };

        Result<QueryingModel> result = parameters.ParseAll();

        result.IsSuccess.Should().BeTrue();
        result.Value.Filter.IsEmpty.Should().BeFalse();
        result.Value.Search.IsEmpty.Should().BeFalse();
        result.Value.Sort.IsEmpty.Should().BeFalse();
        result.Value.Page.IsEmpty.Should().BeFalse();
    }

    [Fact(DisplayName = "ParseAll: Null page with page size uses default page")]
    public void ParseAll_NullPageWithPageSize_ShouldUseDefaults()
    {
        QueryingParameters parameters = QueryingParameters.Empty with { PageNumber = null, PageSize = 25 };

        Result<QueryingModel> result = parameters.ParseAll();

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Page.Should().Be(PageBounds.DefaultPageValue);
        result.Value.Page.PageSize.Should().Be(25);
        result.Value.Page.IsEmpty.Should().BeFalse();
    }

    [Fact(DisplayName = "ParseAll: Negative page clamps to default")]
    public void ParseAll_NegativePage_ShouldClampToDefault()
    {
        QueryingParameters parameters = QueryingParameters.Empty with { PageNumber = -1, PageSize = 10 };

        Result<QueryingModel> result = parameters.ParseAll();

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Page.Should().Be(PageBounds.DefaultPageValue);
        result.Value.Page.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "ParseAll: Negative page size clamps to default")]
    public void ParseAll_NegativePageSize_ShouldClampToDefault()
    {
        QueryingParameters parameters = QueryingParameters.Empty with { PageNumber = 2, PageSize = -5 };

        Result<QueryingModel> result = parameters.ParseAll();

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Page.Should().Be(2);
        result.Value.Page.PageSize.Should().Be(PageBounds.DefaultPageSizeValue);
    }

    [Fact(DisplayName = "ParseAll: Zero page and size clamp to defaults")]
    public void ParseAll_ZeroPageAndSize_ShouldClampToDefaults()
    {
        QueryingParameters parameters = QueryingParameters.Empty with { PageNumber = 0, PageSize = 0 };

        Result<QueryingModel> result = parameters.ParseAll();

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Page.Should().Be(PageBounds.DefaultPageValue);
        result.Value.Page.PageSize.Should().Be(PageBounds.DefaultPageSizeValue);
    }

    [Fact(DisplayName = "ParseAll: Excessive page size clamps to max")]
    public void ParseAll_ExcessivePageSize_ShouldClampToMax()
    {
        QueryingParameters parameters = QueryingParameters.Empty with { PageNumber = 1, PageSize = 999 };

        Result<QueryingModel> result = parameters.ParseAll();

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.PageSize.Should().Be(PageBounds.DefaultMaxPageSizeValue);
    }
}
