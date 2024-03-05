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
    public class CardWriteRepository : WriteRepository<Card>, ICardWriteRepository
    {
        public CardWriteRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
    
}
