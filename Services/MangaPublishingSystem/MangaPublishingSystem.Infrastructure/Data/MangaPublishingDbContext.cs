using Microsoft.EntityFrameworkCore;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data
{
    public class MangaPublishingDbContext : DbContext
    {
        public MangaPublishingDbContext()
        {
        }

        public MangaPublishingDbContext(DbContextOptions<MangaPublishingDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MangaPublishingDbContext).Assembly);
        }

        // VNPay transaction table
        public DbSet<VNPayTransaction> VNPayTransactions { get; set; }
    }
}
