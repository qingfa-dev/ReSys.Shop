namespace Shared.Operational.Persistence.Specifications.Paging;

/// <summary>
/// The unified, parsed and normalized representation of a pagination request.
/// </summary>
/// <remarks>
/// <c>PageModel</c> is the root output of all three input surfaces — explicit values,
/// JSON, and query-string — via the factory methods in <see cref="PageModelExtensions"/>.
/// <para>
/// <c>Page</c> and <c>PageSize</c> are always valid after construction: page is ≥ 1 and
/// page size is clamped to [1, <see cref="Bounds"/>.<see cref="PageBounds.MaxPageSize"/>].
/// Raw caller input is never stored — only the normalized result.
/// </para>
/// <para>
/// Unlike filter, search, and sort models, paging has no field whitelist and no
/// <c>Violations</c> collection. Bounds violations are corrected silently at parse time
/// (clamping), consistent with standard HTTP API conventions where out-of-range pagination
/// parameters are normalized rather than rejected.
/// </para>
/// </remarks>
public sealed partial class PageModel
{
    #region Properties

    /// <summary>
    /// Gets the normalized page number. Always ≥ 1.
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Gets the normalized page size. Always in [1, <see cref="Bounds"/>.<see cref="PageBounds.MaxPageSize"/>].
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the <see cref="PageBounds"/> that were applied when normalizing this model.
    /// </summary>
    public PageBounds Bounds { get; }

    /// <summary>
    /// Gets the original raw input string that produced this model, if available.
    /// Used for diagnostics and cache key correlation.
    /// </summary>
    public string? RawInput { get; }

    /// <summary>
    /// Gets a value indicating whether this model represents an explicit "no pagination" sentinel —
    /// i.e. the caller passed no page or size parameters and the endpoint is configured to
    /// return all results in that case.
    /// </summary>
    /// <remarks>
    /// The expression builder (<c>ToPagedOrAllAsync</c> / <c>ToPagedOrEmptyAsync</c>) inspects
    /// this flag to decide whether to skip pagination entirely. An empty model still carries
    /// normalized <see cref="Page"/> and <see cref="PageSize"/> values from <see cref="Bounds.DefaultPage"/>
    /// and <see cref="Bounds.DefaultPageSize"/> so it remains safe to pass to any overload.
    /// </remarks>
    public bool IsEmpty { get; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new <see cref="PageModel"/> with already-normalized page and size values.
    /// </summary>
    /// <param name="page">The normalized page number (must be ≥ 1).</param>
    /// <param name="pageSize">The normalized page size (must be ≥ 1).</param>
    /// <param name="bounds">The bounds that were applied during normalization.</param>
    /// <param name="isEmpty">
    /// <see langword="true"/> when no pagination parameters were supplied by the caller.
    /// </param>
    /// <param name="rawInput">The original input string, for diagnostics.</param>
    internal PageModel(
        int page,
        int pageSize,
        PageBounds bounds,
        bool isEmpty  = false,
        string? rawInput = null)
    {
        Page = page;
        PageSize = pageSize;
        Bounds = bounds;
        IsEmpty = isEmpty;
        RawInput = rawInput;
    }

    #endregion Constructor

    #region Static Sentinels

    /// <summary>
    /// Returns a page model using <see cref="PageBounds.Default"/> with no explicit caller input.
    /// <see cref="IsEmpty"/> is <see langword="true"/> on the returned instance.
    /// </summary>
    public static PageModel Empty { get; } = new(
        page:     PageBounds.DefaultPageValue,
        pageSize: PageBounds.DefaultPageSizeValue,
        bounds:   PageBounds.Default,
        isEmpty:  true);

    #endregion Static Sentinels
}
