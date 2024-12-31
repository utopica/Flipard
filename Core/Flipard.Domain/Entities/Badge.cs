using Flipard.Domain.Common;
using Flipard.Domain.Enums;

namespace Flipard.Domain.Entities;

public class Badge : EntityBase<Guid>
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public BadgeType Type { get; set; }
    public BadgeRarity Rarity { get; set; }
    public ICollection<UserBadge>? UserBadges { get; set; }
}