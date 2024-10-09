using Flipard.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Domain.Entities;

public class Vocabulary : EntityBase<Guid>
{
    public string Term { get; set; }
    public string Meaning { get; set; }

    public Card Card { get; set; }
    public Guid CardId { get; set; }
}