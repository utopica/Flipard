using Flipard.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Domain.Entities
{
    public class Card : EntityBase<Guid>
    {        
        public DateTimeOffset? LastReviewed { get; set; }
        public Vocabulary Vocabulary { get; set; }
        public Deck Deck { get; set; }
        public Guid DeckId { get; set; }
        public string? ImageUrl { get; set; }
    }
}

