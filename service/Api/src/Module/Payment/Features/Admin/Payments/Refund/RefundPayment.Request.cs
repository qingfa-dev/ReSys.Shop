namespace Module.Payment.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    public class Request
    {
        // [WIP-MVP] Amount parameter is accepted but ignored; always refunds the full captured total.
        // TODO [v1.x]: Implement partial refund. See docs/superpowers/specs/2026-07-07-mvp-cut-design.md.
        public decimal Amount { get; init; }
        public string? Reason { get; init; }
    }
}
