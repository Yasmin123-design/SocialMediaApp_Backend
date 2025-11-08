using FeedService.Models;
using Microsoft.EntityFrameworkCore;

namespace FeedService.Data
{
    public class FeedDbContext : DbContext
    {
        public FeedDbContext(DbContextOptions<FeedDbContext> options) : base(options)
        {
        }

        public DbSet<FeedPost> FeedPosts { get; set; }
    }
}
