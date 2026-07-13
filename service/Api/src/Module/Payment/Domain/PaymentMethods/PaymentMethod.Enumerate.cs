// Context: Controls where a payment method is displayed — Both, Frontend (storefront), or Backend (admin)
namespace Module.Payment.Domain.PaymentMethods;

public enum DisplayOn
{
    Both,
    Frontend,
    Backend
}