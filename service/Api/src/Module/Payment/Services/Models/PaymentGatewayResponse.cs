namespace Module.Payment.Services.Models;

/// <summary>Gateway response data — success/failure carried by Result wrapper, not this record.</summary>
public sealed record PaymentGatewayResponse
{
    public string Provider { get; }
    public string? Authorization { get; }
    public string? ClientSecret { get; }
    public string? SetupIntentClientSecret { get; }
    public string? PaymentStatus { get; }
    public string? AvsResultCode { get; }
    public string? CvvResultCode { get; }
    public string? CvvResultMessage { get; }
    public Dictionary<string, object?> Properties { get; }

    public PaymentGatewayResponse(
        string provider,
        string? authorization = null,
        string? clientSecret = null,
        string? setupIntentClientSecret = null,
        string? paymentStatus = null,
        Dictionary<string, object?>? properties = null,
        string? avsResultCode = null,
        string? cvvResultCode = null,
        string? cvvResultMessage = null)
    {
        Provider = provider;
        Authorization = authorization;
        ClientSecret = clientSecret;
        SetupIntentClientSecret = setupIntentClientSecret;
        PaymentStatus = paymentStatus;
        Properties = properties ?? new Dictionary<string, object?>();
        AvsResultCode = avsResultCode;
        CvvResultCode = cvvResultCode;
        CvvResultMessage = cvvResultMessage;
    }
}
