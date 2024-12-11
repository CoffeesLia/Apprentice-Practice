using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Context
{
    public class CleanArchBaseContext(DbContextOptions<CleanArchBaseContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
            base.OnModelCreating(modelBuilder);

     
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
        }

        public DbSet<PartNumber> PartNumber { get; set; }
        public DbSet<Supplier> Supplier { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<PartNumberSupplier> PartNumberSupplier { get; set; }
        public DbSet<PartNumberVehicle> PartNumberVehicle { get; set; }
    }
}
