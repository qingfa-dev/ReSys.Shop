namespace Module.Payment.Domain.Gateways;
/// <summary>Represents a Payment Gateway Response.</summary>

// Invariant: Options contains authorization/avs_result/cvv_result when present; Params holds raw gateway response
// Define: Gateway response wrapper ported from Spree::PaymentResponse
public sealed record PaymentGatewayResponse
{
    public bool Success { get; }
    public string Message { get; }
    public IReadOnlyDictionary<string, object> Params { get; }
    public IReadOnlyDictionary<string, object> Options { get; }
    public string? Authorization { get; }
    public IReadOnlyDictionary<string, string>? AvsResult { get; }
    public IReadOnlyDictionary<string, string>? CvvResult { get; }
    public string? CvvCode { get; }
    public string? CvvMessage { get; }

    // Contract: pre=success!=null, post=this.Success==success && this.Message==message
    public PaymentGatewayResponse(
        bool success,
        string message,
        Dictionary<string, object>? parmas = null,
        Dictionary<string, object>? options = null,
        string? authorization = null,
        Dictionary<string, string>? avsResult = null,
        Dictionary<string, string>? cvvResult = null,
        string? cvvCode = null,
        string? cvvMessage = null)
    {
        Success = success;
        Message = message;
        Params = parmas ?? new Dictionary<string, object>();
        Options = options ?? new Dictionary<string, object>();
        Authorization = authorization;
        AvsResult = avsResult;
        CvvResult = cvvResult;
        CvvCode = cvvCode;
        CvvMessage = cvvMessage;
    }

    // Compute: Extract AVS code from AVS result dictionary; defaults to empty string when not present — mirrors Ruby avs_result['code']
    public string AvsResultCode => AvsResult?.GetValueOrDefault("code") ?? string.Empty;

    // Compute: Extract CVV result code; defaults to null when not present — mirrors Ruby cvv_result['code']
    public string? CvvResultCode => CvvResult?.GetValueOrDefault("code");

    // Compute: Extract CVV result message; defaults to null when not present — mirrors Ruby cvv_result['message']
    public string? CvvResultMessage => CvvResult?.GetValueOrDefault("message");
}