using Flipard.Domain.Common;
using Flipard.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Domain.Identity;

public class User : IdentityUser<Guid>, IFullAuditedEntity
{
    public DateTimeOffset? Birthdate { get; set; }
    public UserSetting UserSetting { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
    public string ModifiedByUserId { get; set; }
    public DateTimeOffset? DeletedOn { get; set; }
    public string? DeletedByUserId { get; set; }
    public bool? IsDeleted { get; set; }
}