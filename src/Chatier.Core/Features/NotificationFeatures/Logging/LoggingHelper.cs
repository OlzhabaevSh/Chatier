using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.NotificationFeatures.Logging;

public static class LoggingHelper
{
    public static InformationLogger Information(
        this ILogger<NotificationGrain> logger) =>
            new(logger);

    public static WarningLogger Warnings(
        this ILogger<NotificationGrain> logger) =>
            new(logger);

    public static ErrorLogger Errors(
        this ILogger<NotificationGrain> logger) =>
            new(logger);
}

public record InformationLogger(ILogger<NotificationGrain> Logger);

public record WarningLogger(ILogger<NotificationGrain> Logger);

public record ErrorLogger(ILogger<NotificationGrain> Logger);
