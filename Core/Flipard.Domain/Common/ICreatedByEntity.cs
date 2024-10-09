using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Domain.Common;

public interface ICreatedByEntity
{
    public DateTimeOffset CreatedOn { get; set; }
    string CreatedByUserId { get; set; }
}