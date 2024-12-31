using Flipard.Domain.Common;
using Flipard.Domain.Identity;

namespace Flipard.Domain.Entities;

public class UserBadge : EntityBase<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid BadgeId { get; set; }
    public Badge Badge { get; set; }
    public DateTimeOffset AwardedDate { get; set; }
}