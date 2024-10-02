﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shoe_Store_HandleAPI.Models;

namespace Shoe_Store_HandleAPI.Config
{
    public class RoleConfig : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.RoleId);

            builder.Property(r => r.RoleName)
                .IsRequired();

            builder.HasMany(r => r.Clients)
                .WithOne(c => c.Role)
                .HasForeignKey(c => c.RoleId);

            builder.HasMany(r => r.Admins)
                .WithOne(a => a.Role)
                .HasForeignKey(a => a.RoleId);
        }
    }
}
