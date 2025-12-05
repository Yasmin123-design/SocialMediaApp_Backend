using ChatService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ChatService.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        public DbSet<MessageThread> MessageThreads { get; set; } = null!;
        public DbSet<MessageThreadParticipant> MessageThreadParticipants { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MessageThreadParticipant>()
                .HasIndex(p => new { p.ThreadId, p.UserId })
                .IsUnique();

            modelBuilder.Entity<MessageThreadParticipant>()
                .HasIndex(p => p.UserId);

            modelBuilder.Entity<MessageThread>()
                .HasIndex(t => t.LastMessageAt);

            modelBuilder.Entity<MessageThread>()
                .HasMany(t => t.Participants)
                .WithOne(p => p.Thread)
                .HasForeignKey(p => p.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MessageThread>()
                .HasMany(t => t.Messages)
                .WithOne(m => m.Thread)
                .HasForeignKey(m => m.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
