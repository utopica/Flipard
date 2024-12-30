using Flipard.Domain.Entities;
using Flipard.Domain.Enums;

namespace Flipard.Domain.Helpers;

public static class BadgeDefinitions
{
    public static readonly Badge FirstDeckCreator = new Badge
    {
        Name = "Deck Master Initiate",
        Description = "Created your first deck!",
        Type = BadgeType.Achievement,
        Rarity = BadgeRarity.Common
    };

    public static readonly Badge TrueChampion = new Badge
    {
        Name = "True Champion",
        Description = "Completed a quiz with no mistakes!",
        Type = BadgeType.Performance,
        Rarity = BadgeRarity.Rare
    };

    public static readonly Badge StreakMaster = new Badge
    {
        Name = "Streak Master",
        Description = "Maintained a 3-day learning streak!",
        Type = BadgeType.Streak,
        Rarity = BadgeRarity.Uncommon
    };
}