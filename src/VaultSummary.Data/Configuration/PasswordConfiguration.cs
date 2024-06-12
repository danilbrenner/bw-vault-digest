using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultSummary.Data.DataModel;

namespace VaultSummary.Data.Configuration;

public class PasswordConfiguration : IEntityTypeConfiguration<Password>
{
    public void Configure(EntityTypeBuilder<Password> builder)
    {
        builder.ToTable("Passwords").HasKey(p => p.Id);
        builder.Property(p => p.LoginId).IsRequired();
        builder.Property(p => p.Strength).IsRequired();
        builder.Property(p => p.LastUsedDate).IsRequired();
        builder.Property(p => p.Hash).IsRequired().HasMaxLength(80);
    }
}