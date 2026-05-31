using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Application.Settings.Commands.UpsertSetting;

public class UpsertSettingCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UpsertSettingCommand, SettingDto>
{
    public async ValueTask<SettingDto> Handle(UpsertSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == currentUser.TenantId && s.Key == request.Key, cancellationToken);

        if (setting is null)
        {
            setting = TenantSetting.Create(currentUser.TenantId, request.Key, request.Value);
            db.TenantSettings.Add(setting);
        }
        else
        {
            setting.UpdateValue(request.Value);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new SettingDto(setting.Key, setting.Value, setting.UpdatedAt);
    }
}
