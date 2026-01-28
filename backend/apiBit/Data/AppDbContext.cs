using apiBit.API.Models;
using apiBit.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace apiBit.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<PersonAddress> PersonAddresses { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyAddress> CompanyAddresses { get; set; }
        public DbSet<Plan> Plans { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Person>()
                .HasIndex(p => p.Document)
                .IsUnique();

            builder.Entity<Person>()
                .HasMany(p => p.Addresses)
                .WithOne(a => a.Person)
                .HasForeignKey(a => a.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

                builder.Entity<Company>()
                    .HasIndex(c => c.Document)
                    .IsUnique();

                builder.Entity<Company>()
                    .HasMany(c => c.Addresses)
                    .WithOne(a => a.Company)
                    .HasForeignKey(a => a.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                    }
    }
}