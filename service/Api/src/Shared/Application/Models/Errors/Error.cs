namespace Shared.Application.Models.Errors;

public readonly partial struct Error
{
    public string Code { get; init; }
    public string Message { get; init; }
    public string Description => Message;
    public int Type { get; init; }
    public Dictionary<string, object?>? Metadata { get; init; }

    private Error(
        string code,
        string message,
        int type,
        Dictionary<string, object?>? metadata = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Metadata = metadata;
    }
}