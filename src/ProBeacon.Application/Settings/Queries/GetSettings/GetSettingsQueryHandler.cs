using MediatR;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Settings.Queries.GetSettings;

public class GetSettingsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetSettingsQuery, List<SettingDto>>
{
    public async Task<List<SettingDto>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
        => await db.TenantSettings
            .Where(s => s.TenantId == currentUser.TenantId)
            .Select(s => new SettingDto(s.Key, s.Value, s.UpdatedAt))
            .ToListAsync(cancellationToken);
}
