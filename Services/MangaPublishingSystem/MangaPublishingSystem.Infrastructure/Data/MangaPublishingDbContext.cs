using Microsoft.EntityFrameworkCore;
using MangaPublishingSystem.Domain.Entities;

using MangaPublishingSystem.Domain.Common;

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

        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public virtual DbSet<Wallet> Wallets { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
        public virtual DbSet<AssistantProfile> AssistantProfiles { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<Series> Series { get; set; } = null!;
        public virtual DbSet<RankingRecord> RankingRecords { get; set; } = null!;
        public virtual DbSet<BoardVote> BoardVotes { get; set; } = null!;
        public virtual DbSet<Contract> Contracts { get; set; } = null!;
        public virtual DbSet<ContractAddendum> ContractAddendums { get; set; } = null!;
        public virtual DbSet<Chapter> Chapters { get; set; } = null!;
        public virtual DbSet<Page> Pages { get; set; } = null!;
        public virtual DbSet<Region> Regions { get; set; } = null!;
        public virtual DbSet<Tasks> Tasks { get; set; } = null!;
        public virtual DbSet<TaskVersion> TaskVersions { get; set; } = null!;
        public virtual DbSet<DisputeLog> DisputeLogs { get; set; } = null!;
        public virtual DbSet<Annotation> Annotations { get; set; } = null!;
        public virtual DbSet<Report> Reports { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MangaPublishingDbContext).Assembly);

            // Configure CreateAt and UpdateAt globally for all entities inheriting from BaseEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseEntity.CreateAt))
                        .HasDefaultValueSql("GETUTCDATE()")
                        .IsRequired();

                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseEntity.UpdateAt))
                        .IsRequired(false);
                }
            }
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreateAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdateAt = DateTime.UtcNow;
                }
            }
        }

    }
}
