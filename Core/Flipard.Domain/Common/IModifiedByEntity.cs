using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Domain.Common;

public interface IModifiedByEntity
{
    public DateTimeOffset? ModifiedOn { get; set; }
    string? ModifiedByUserId { get; set; }
}