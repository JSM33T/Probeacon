using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProBeacon.Domain.Entities;

namespace ProBeacon.Infrastructure.Persistence.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.RefreshTokenHash)
            .IsRequired();

        builder.Property(s => s.UserAgent)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.IpAddress)
            .HasMaxLength(45)
            .IsRequired();

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.RefreshTokenHash);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
