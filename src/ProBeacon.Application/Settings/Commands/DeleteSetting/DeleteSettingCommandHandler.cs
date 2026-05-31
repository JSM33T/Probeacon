using Mediator;
using Microsoft.EntityFrameworkCore;
using ProBeacon.Application.Common.Interfaces;

namespace ProBeacon.Application.Settings.Commands.DeleteSetting;

public class DeleteSettingCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : ICommandHandler<DeleteSettingCommand>
{
    public async ValueTask<Unit> Handle(DeleteSettingCommand request, CancellationToken cancellationToken)
    {
        var key = request.Key.Trim();

        // Only "general" (non-namespaced) settings can be deleted here. Namespaced
        // settings such as smtp.* are managed by their dedicated configuration pages.
        if (key.Contains('.'))
            throw new InvalidOperationException($"'{key}' is a managed setting and cannot be deleted here.");

        var setting = await db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == currentUser.TenantId && s.Key == key, cancellationToken);

        if (setting is null)
            throw new KeyNotFoundException($"Setting '{key}' was not found.");

        db.TenantSettings.Remove(setting);
        await db.SaveChangesAsync(cancellationToken);

        return default;
    }
}
