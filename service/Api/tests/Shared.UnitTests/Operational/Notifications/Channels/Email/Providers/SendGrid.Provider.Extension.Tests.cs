using Microsoft.AspNetCore.Builder;

using Shared.Operational.Notifications.Channels.Emails.Providers.SendGrid;
using Shared.Operational.Notifications.Providers;

namespace Shared.UnitTests.Operational.Notifications.Channels.Email.Providers;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Notifications")]
public sealed class SendGridProviderExtensionTests
{
    [Fact(DisplayName = "AddEmailSendGridProvider should register SendGridProvider as INotificationProvider")]
    public void AddEmailSendGridProvider_ShouldRegisterProvider()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.AddEmailSendGridProvider();

        builder.Services.Should().Contain(
            s => s.ServiceType == typeof(INotificationProvider)
                 && s.ImplementationType == typeof(SendGridProvider));
    }
}
