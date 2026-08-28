using Localizr.Infrastructure.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Localizr.Infrastructure.Persistence.Configurations;

internal sealed class IdentityConfigurations : 
IEntityTypeConfiguration<User>,
IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
    }

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
    }

}
