using BuildingBlocks.Mediators.Events;
using BuildingBlocks.Notifications.Models;
using BuildingBlocks.Notifications.Services;
using BuildingBlocks.Notifications.Templates;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.Shipments.Events;

namespace Module.Shipping.Infrastructure.Notifications;

public sealed class ShipmentShippedConsumer(
    ILogger<ShipmentShippedConsumer> logger,
    INotificationService notificationService)
    : DomainEventConsumer<ShipmentEvent.Lifecycle.Shipped>
{
    public override async Task Handle(ShipmentEvent.Lifecycle.Shipped notification, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Shipment shipped: {ShipmentId}, Order: {OrderId}, Tracking: {TrackingNumber}, Carrier: {Carrier}",
                notification.ShipmentId, notification.OrderId, notification.TrackingNumber, notification.Carrier);
        }

        var message = NotificationMessage.Create(
            NotificationTemplateUseCase.SystemOrderShipped,
            NotificationRecipient.Create(notification.CustomerEmail, notification.OrderId.ToString()),
            NotificationContext.Create(
                (NotificationParameterType.OrderId, notification.OrderId.ToString()),
                (NotificationParameterType.OrderTrackingNumber, notification.TrackingNumber),
                (NotificationParameterType.UserFirstName, notification.CustomerEmail.Split('@')[0])));

        var result = await notificationService.SendAsync(message, cancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to send shipment shipped notification for order {OrderId}: {Errors}",
                notification.OrderId,                 string.Join("; ", result.Failures.Select(f => f.Description)));
        }
    }
}
