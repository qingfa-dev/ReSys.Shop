using Microsoft.AspNetCore.Builder;

using Shared.Operational.Notifications.Channels.Emails.Providers.Smtp;
using Shared.Operational.Notifications.Providers;

namespace Shared.UnitTests.Operational.Notifications.Channels.Email.Providers;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Notifications")]
public sealed class SmtpProviderExtensionTests
{
    [Fact(DisplayName = "AddEmailSmtpProvider should register SmtpProvider as INotificationProvider")]
    public void AddEmailSmtpProvider_ShouldRegisterProvider()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.AddEmailSmtpProvider();

        builder.Services.Should().Contain(
            s => s.ServiceType == typeof(INotificationProvider)
                 && s.ImplementationType == typeof(SmtpProvider));
    }
}
