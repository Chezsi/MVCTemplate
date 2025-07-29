using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVCTemplate.Models; 

namespace MVCtemplate.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categorys { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Site> Sites { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            /*builder.Entity<Contract>()
                .HasOne(c => c.Person)
                .WithMany()
                .HasForeignKey(c => c.PersonId)
                .OnDelete(DeleteBehavior.Cascade);*/

            builder.Entity<Person>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }
}
