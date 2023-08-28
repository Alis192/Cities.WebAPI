using CitiesManager.Core.Entities;
using CitiesManager.Core.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CitiesManager.Infrastructure.DatabaseContext

{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        public ApplicationDbContext()
        {
        }

        public virtual DbSet<City> Cities { get; set; }

        //To create sample seed data
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //This city object is going to be inserted automatically when a new city stable is created by using update database command
            modelBuilder.Entity<City>().HasData(new City() { CityID = Guid.Parse("2D827F3B-D7AB-41F4-AF69-995D4588AC7E"), CityName = "Baku" });

            modelBuilder.Entity<City>().HasData(new City() { CityID = Guid.Parse("3CB7F21D-553B-4222-B4D0-91C8D4E9DBDC"), CityName = "London" });
        }
    }
}
