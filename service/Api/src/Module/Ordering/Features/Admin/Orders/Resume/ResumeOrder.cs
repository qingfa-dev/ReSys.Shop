using Module.Ordering.Domain.Orders;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

namespace Module.Ordering.Features.Admin.Orders.Resume;

/// <summary>Resumes a previously canceled order.</summary>
public static partial class ResumeOrder
{
    public class Response { public Guid Id { get; init; } public OrderStatus Status { get; init; } }
    public sealed record Command(Guid Id) : ICommand<Response>;
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        INotificationService notificationService,
        ILogger<CommandHandler> logger) : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null) return (Result<Response>)OrderResult.Errors.NotFound(command.Id);

            var result = order.Resume();
            if (result.IsFailure) return (Result<Response>)result.Failures;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Notify: Send order resumed notification.
            await SendOrderResumedNotificationAsync(order, cancellationToken);

            return new Response { Id = order.Id, Status = order.Status };
        }

        private async Task SendOrderResumedNotificationAsync(Order order, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(order.Email))
                return;

            var message = NotificationMessage.Create(
                NotificationUseCase.OrderConfirmed,
                NotificationRecipient.Create(order.Email, order.Number),
                NotificationChannel.Email,
                NotificationContext.Create(
                    (NotificationParameterType.OrderNumber, order.Number),
                    (NotificationParameterType.UserFirstName, order.Email.Split('@')[0])));

            var result = await notificationService.SendAsync(message, ct);
            if (result.IsFailure)
            {
                logger.LogWarning("Failed to send order resumed notification for order {OrderId}: {Errors}",
                    order.Id, string.Join("; ", result.Failures.Select(f => f.Description)));
            }
        }
    }
}
