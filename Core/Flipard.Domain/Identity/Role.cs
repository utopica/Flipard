using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flipard.Domain.Common;

namespace Flipard.Domain.Identity;

public class Role : IdentityRole<Guid>, IFullAuditedEntity
{
    public string? Description { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
    public string? ModifiedByUserId { get; set; }
    public DateTimeOffset? DeletedOn { get; set; }
    public string? DeletedByUserId { get; set; }
    public bool? IsDeleted { get; set; }

    public Role()
    {
        Id = Guid.NewGuid();
        CreatedOn = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }
}