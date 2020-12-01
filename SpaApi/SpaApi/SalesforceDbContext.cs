using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpaApi
{
    public class SalesforceDbContext: DbContext
    {
        private readonly IConfiguration _config;
        private readonly HerokuClient _client;

        public DbSet<Contact> Contacts { get; set; }

        public SalesforceDbContext(IConfiguration config, HerokuClient client)
        {
            _config = config;
            _client = client;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                var builder = _client.GetConnectionString().GetAwaiter().GetResult();
                optionsBuilder.UseNpgsql(builder.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("salesforce");
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.ToTable("contact");
                entity.HasKey(c => c.id);
            });
        }
    }
}
