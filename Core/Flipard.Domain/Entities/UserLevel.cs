using Flipard.Domain.Common;
using Flipard.Domain.Identity;

namespace Flipard.Domain.Entities;

public class UserLevel : EntityBase<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public int Level { get; set; }
    public int CurrentExperience { get; set; } // XP Count
    public int RequiredExperience { get; set; } // Max Xp Count
    public DateTimeOffset LastLevelUpDate { get; set; }
    public int CurrentStreak { get; set; }
    public DateTimeOffset LastActivityDate { get; set; }
}