using Flipard.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Domain.Entities;

public class Deck : EntityBase<Guid>
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Card> Cards { get; set; }
}