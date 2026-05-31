using Microsoft.EntityFrameworkCore;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<TenantSetting> TenantSettings { get; }
    DbSet<User> Users { get; }
    DbSet<UserSession> UserSessions { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
