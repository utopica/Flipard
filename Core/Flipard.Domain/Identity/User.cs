using Flipard.Domain.Common;
using Flipard.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Domain.Identity
{
    public class User :IdentityUser<Guid>, IEntityBase<Guid>, ICreatedByEntity, IModifiedByEntity
    {
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        public DateTimeOffset? Birthdate { get; set; }
        //public Gender Gender { get; set; }
        public UserSetting UserSetting { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public string ModifiedByUserId { get; set; }
    }
}
