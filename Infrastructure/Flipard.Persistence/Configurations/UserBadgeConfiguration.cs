﻿using Flipard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flipard.Persistence.Configurations;

public class UserBadgeConfiguration : IEntityTypeConfiguration<UserBadge>
{
    public void Configure(EntityTypeBuilder<UserBadge> builder)
    {
        builder.HasKey(qa => qa.Id);
        
        // COMMON FIELDS

        // CreatedByUserId
        builder.Property(x => x.CreatedByUserId).IsRequired();
        builder.Property(x => x.CreatedByUserId).HasMaxLength(75);

        // CreatedOn
        builder.Property(x => x.CreatedOn).IsRequired();

        // ModifiedByUserId
        builder.Property(x => x.ModifiedByUserId).IsRequired(false);
        builder.Property(x => x.ModifiedByUserId).HasMaxLength(75);

        // LastModifiedOn
        builder.Property(x => x.ModifiedOn).IsRequired(false);

        // DeletedByUserId
        builder.Property(x => x.DeletedByUserId).IsRequired(false);
        builder.Property(x => x.DeletedByUserId).HasMaxLength(75);

        // DeletedOn
        builder.Property(x => x.DeletedOn).IsRequired(false);

        // IsDeleted
        builder.Property(x => x.IsDeleted).IsRequired(false);
        
        builder.ToTable("UserBadges");
    }
    
}