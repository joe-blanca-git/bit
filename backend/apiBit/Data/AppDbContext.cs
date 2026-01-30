using apiBit.Models;
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
        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicationMenu> ApplicationMenus { get; set; }
        public DbSet<ApplicationSubMenu> ApplicationSubMenus { get; set; }
        public DbSet<PlanApplication> PlanApplications { get; set; }
        public DbSet<PlanMenu> PlanMenus { get; set; }
        public DbSet<PlanSubMenu> PlanSubMenus { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

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


            builder.Entity<Application>()
                .HasMany(a => a.Menus)
                .WithOne(m => m.Application)
                .HasForeignKey(m => m.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationMenu>()
                .HasMany(m => m.SubMenus)
                .WithOne(s => s.ApplicationMenu)
                .HasForeignKey(s => s.ApplicationMenuId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PlanApplication>()
                .HasIndex(p => new { p.PlanId, p.ApplicationId })
                .IsUnique();

            builder.Entity<PlanMenu>()
                .HasIndex(p => new { p.PlanId, p.ApplicationMenuId })
                .IsUnique();
                
            builder.Entity<PlanSubMenu>()
                .HasIndex(p => new { p.PlanId, p.ApplicationSubMenuId })
                .IsUnique();

            builder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Order>()
                .HasMany(o => o.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                        
            }
                        
    }
}