using Flipard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Persistence.Configurations
{
    public class DeckConfiguration : IEntityTypeConfiguration<Deck>
    {
        public void Configure(EntityTypeBuilder<Deck> builder)
        {
            // Primary key
            builder.HasKey(u => u.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

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
            builder.Property(x => x.IsDeleted).IsRequired();

            // Properties

            // Name
            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(250);

            // Description 
            builder.Property(x => x.Name).IsRequired(false);
            builder.Property(x => x.Name).HasMaxLength(500);

            // Relationships

            builder.HasMany(x => x.Cards)
                .WithOne(x => x.Deck)
                .HasForeignKey(x=> x.DeckId);

            builder.ToTable("Decks");
        }
    }
}
