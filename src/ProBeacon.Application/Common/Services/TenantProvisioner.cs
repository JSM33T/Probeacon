using System.Text.RegularExpressions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Auth.Commands.SendVerificationEmail;
using ProBeacon.Application.Common.Exceptions;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Models;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Common.Services;

public partial class TenantProvisioner(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRequestContext requestContext,
    ISender sender)
    : ITenantProvisioner
{
    public async Task<TenantProvisioningResult> ProvisionAsync(
        TenantProvisioningRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.ToLowerInvariant();
        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
            throw new ConflictException("An account already exists for this email.");

        var slug = await GenerateUniqueSlugAsync(request.OrganizationName, cancellationToken);
        var tenant = Tenant.Create(request.OrganizationName, slug, request.TenantKind, request.ExpiresAt);
        db.Tenants.Add(tenant);

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(tenant.Id, email, request.AdminName, passwordHash);
        db.Users.Add(user);

        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var session = UserSession.Create(
            user.Id,
            tenant.Id,
            tokenService.HashRefreshToken(rawRefreshToken),
            requestContext.UserAgent,
            requestContext.IpAddress);
        db.UserSessions.Add(session);

        await db.SaveChangesAsync(cancellationToken);

        var token = tokenService.GenerateAccessToken(user, tenant, session.Id);

        await sender.Send(new SendVerificationEmailCommand(user.Id), cancellationToken);

        return new TenantProvisioningResult(
            token.AccessToken,
            token.ExpiresAt,
            rawRefreshToken,
            session.Id,
            tenant.Id,
            tenant.Slug,
            tenant.Kind,
            tenant.ExpiresAt,
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role.ToString());
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = SlugInvalidCharactersRegex()
            .Replace(name.Trim().ToLowerInvariant(), "-")
            .Trim('-');

        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = "workspace";

        baseSlug = baseSlug.Length > 80 ? baseSlug[..80].Trim('-') : baseSlug;

        for (var attempt = 0; attempt < 20; attempt++)
        {
            var slug = attempt == 0
                ? baseSlug
                : $"{baseSlug}-{Guid.NewGuid():N}"[..Math.Min(baseSlug.Length + 9, 120)];

            if (!await db.Tenants.AnyAsync(tenant => tenant.Slug == slug, cancellationToken))
                return slug;
        }

        return $"workspace-{Guid.NewGuid():N}";
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex SlugInvalidCharactersRegex();
}
