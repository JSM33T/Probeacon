using System.Globalization;
using System.Threading.RateLimiting;

namespace ProBeacon.Api.RateLimiting;

/// <summary>
/// Per-IP token-bucket rate limiting for the unauthenticated auth surface. Buckets are keyed by
/// the client's connection IP, so each caller gets its own allowance: a short burst followed by a
/// steady refill. Exhausting a bucket returns 429 with a <c>Retry-After</c> header.
/// </summary>
/// <remarks>
/// The partition key is the TCP peer address (<see cref="Microsoft.AspNetCore.Http.ConnectionInfo.RemoteIpAddress"/>),
/// which a caller cannot spoof — unlike <c>X-Forwarded-For</c>. When deployed behind a reverse
/// proxy, configure forwarded-headers so the real client IP reaches the app (the session-IP
/// tracking in <c>RequestContext</c> has the same dependency); otherwise every client shares the
/// proxy's bucket.
/// </remarks>
public static class RateLimitPolicies
{
    /// <summary>Login, signup, refresh, set-password — interactive auth, generous burst.</summary>
    public const string Auth = "auth";

    /// <summary>Forgot-password, verify-email, send-verification — email triggers, tighter to curb bombing.</summary>
    public const string AuthSensitive = "auth-sensitive";

    public static IServiceCollection AddProBeaconRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Burst of 10, then ~2 tokens back every 10s (≈12/min) — comfortable for real users
            // (multi-tab token refresh, a few login retries) but a hard wall to credential stuffing.
            options.AddPolicy(Auth, httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(
                    ClientPartitionKey(httpContext),
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 10,
                        TokensPerPeriod = 2,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            // Burst of 5, then 1 token back per minute — enough for a genuine resend, stingy enough
            // that nobody can use us to flood an inbox.
            options.AddPolicy(AuthSensitive, httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(
                    ClientPartitionKey(httpContext),
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 5,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(60),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        type = "https://tools.ietf.org/html/rfc6585#section-4",
                        title = "Too Many Requests",
                        status = StatusCodes.Status429TooManyRequests,
                        detail = "Too many requests. Please slow down and try again shortly."
                    },
                    cancellationToken);
            };
        });

        return services;
    }

    private static string ClientPartitionKey(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
