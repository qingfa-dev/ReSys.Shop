namespace Module.Payment.Domain.Gateways;

public sealed record PaymentGatewayResponse
{
    public bool Success { get; }
    public string Message { get; }
    public string Provider { get; }
    public string? Authorization { get; }
    public string? SetupIntentClientSecret { get; }
    public string? PaymentStatus { get; }
    public string? AvsResultCode { get; }
    public string? CvvResultCode { get; }
    public string? CvvResultMessage { get; }
    public Dictionary<string, object?> Properties { get; }

    public PaymentGatewayResponse(
        bool success,
        string message,
        string provider,
        string? authorization = null,
        string? setupIntentClientSecret = null,
        string? paymentStatus = null,
        Dictionary<string, object?>? properties = null,
        string? avsResultCode = null,
        string? cvvResultCode = null,
        string? cvvResultMessage = null)
    {
        Success = success;
        Message = message;
        Provider = provider;
        Authorization = authorization;
        SetupIntentClientSecret = setupIntentClientSecret;
        PaymentStatus = paymentStatus;
        Properties = properties ?? new Dictionary<string, object?>();
        AvsResultCode = avsResultCode;
        CvvResultCode = cvvResultCode;
        CvvResultMessage = cvvResultMessage;
    }
}
