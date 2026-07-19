using Microsoft.EntityFrameworkCore;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Infrastructure.DbContexts;

public class PollDbContext : DbContext
{
    public PollDbContext(DbContextOptions<PollDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<QnAQuestion> QnAQuestions => Set<QnAQuestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AuthProvider).HasMaxLength(50).IsRequired();

            entity.HasMany(e => e.Polls)
                  .WithOne(p => p.CreatedByUser)
                  .HasForeignKey(p => p.CreatedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Votes)
                  .WithOne(v => v.User)
                  .HasForeignKey(v => v.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.QnAQuestions)
                  .WithOne(q => q.User)
                  .HasForeignKey(q => q.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Poll entity configuration
        modelBuilder.Entity<Poll>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasMany(e => e.Options)
                  .WithOne(o => o.Poll)
                  .HasForeignKey(o => o.PollId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Votes)
                  .WithOne(v => v.Poll)
                  .HasForeignKey(v => v.PollId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.QnAQuestions)
                  .WithOne(q => q.Poll)
                  .HasForeignKey(q => q.PollId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // PollOption entity configuration
        modelBuilder.Entity<PollOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OptionText).HasMaxLength(300).IsRequired();
        });

        // Vote entity configuration
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PollId, e.VoterToken });
            entity.Property(e => e.VoterToken).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TextResponse).HasMaxLength(1000);
        });

        // QnAQuestion entity configuration
        modelBuilder.Entity<QnAQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuestionText).HasMaxLength(500).IsRequired();
            entity.Property(e => e.VoterToken).HasMaxLength(100).IsRequired();
        });
    }
}
