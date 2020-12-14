using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Odata.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Odata.Data
{
    public class OdataDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public OdataDbContext(IConfiguration configuration, DbContextOptions<OdataDbContext> options) : base(options)
        {
            _configuration = configuration;
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //        optionsBuilder.UseSqlServer(_configuration.GetConnectionString("EnterpriseDb"));

        //    optionsBuilder.EnableSensitiveDataLogging();
        //    optionsBuilder.EnableDetailedErrors();
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BusinessType>(entity =>
            {
                entity.ToTable("BusinessTypes");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.AttributeValue).IsRequired(true);
                entity.Property(m => m.BmisValue).IsRequired(true);
            });
        }

        public DbSet<Odata.Models.BusinessType> BusinessType { get; set; }
    }
}
