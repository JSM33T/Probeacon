namespace ProBeacon.Application.Common.Exceptions;

/// <summary>
/// Thrown when an action is throttled at the application layer (e.g. account lockout after repeated
/// failed sign-ins). Maps to HTTP 429; <see cref="RetryAfter"/>, when set, drives the Retry-After header.
/// </summary>
public sealed class TooManyRequestsException(string message, TimeSpan? retryAfter = null)
    : Exception(message)
{
    public TimeSpan? RetryAfter { get; } = retryAfter;
}
