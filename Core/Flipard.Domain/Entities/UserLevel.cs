using Flipard.Domain.Common;
using Flipard.Domain.Identity;

namespace Flipard.Domain.Entities;

public class UserLevel : EntityBase<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public int Level { get; set; }
    public int CurrentExperience { get; set; }
    public int RequiredExperience { get; set; }
    public DateTimeOffset LastLevelUpDate { get; set; }
    public int CurrentStreak { get; set; }
    public DateTimeOffset LastActivityDate { get; set; }
}