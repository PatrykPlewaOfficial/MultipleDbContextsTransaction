using Microsoft.EntityFrameworkCore;
using MultipleDbContextsTransaction.Entities;

namespace MultipleDbContextsTransaction.Contexts
{
    internal class My2DbContext : DbContext, ISharedTransactionDbContext
    {
        public My2DbContext(DbContextOptions<My2DbContext> opt) : base(opt)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<Tab2>().ToTable("Tab2");
        }
    }
}
