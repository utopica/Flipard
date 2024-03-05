using Flipard.Application.Repositories;
using Flipard.Domain.Entities;
using Flipard.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Persistence.Repositories
{
    public class DeckWriteRepository : WriteRepository<Deck>, IDeckWriteRepository
    {
        public DeckWriteRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
    
}
