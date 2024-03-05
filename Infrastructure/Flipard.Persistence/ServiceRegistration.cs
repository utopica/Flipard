using Flipard.Application.Repositories;
using Flipard.Persistence.Contexts;
using Flipard.Persistence.Contexts.Identity;
using Flipard.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceServices(this
            IServiceCollection services, IConfiguration configuration)
        {

            var connectionString = configuration.GetConnectionString("PostgreSQL");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            services.AddDbContext<IdentityContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            services.AddScoped<ICardReadRepository, CardReadRepository>();
            services.AddScoped<ICardWriteRepository, CardWriteRepository>();

            services.AddScoped<IDeckReadRepository, DeckReadRepository>();
            services.AddScoped<IDeckWriteRepository, DeckWriteRepository>();

            services.AddScoped<IVocabularyReadRepository, VocabularyReadRepository>();
            services.AddScoped<IVocabularyWriteRepository, VocabularyWriteRepository>();

        }
    }
}
