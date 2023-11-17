using Microsoft.EntityFrameworkCore;
using MultipleDbContextsTransaction.Entities;

namespace MultipleDbContextsTransaction.Contexts
{
    internal class My1DbContext : DbContext, ISharedTransactionDbContext
    {
        public My1DbContext(DbContextOptions<My1DbContext> opt) : base(opt)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<Tab1>().ToTable("Tab1");
        }
    }
}
