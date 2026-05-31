using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Infrastructure.Persistence.Configurations;

public class TenantSettingConfiguration : IEntityTypeConfiguration<TenantSetting>
{
    public void Configure(EntityTypeBuilder<TenantSetting> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Value)
            .IsRequired();

        // one unique key per tenant
        builder.HasIndex(s => new { s.TenantId, s.Key })
            .IsUnique();
    }
}
