using Microsoft.EntityFrameworkCore;
using redsoft.Models;

namespace Models
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
            Database.Migrate();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }


        public DbSet<Account> Account { get; set; }
        public DbSet<AccountHistory> AccountHistory { get; set; }
    }
}
