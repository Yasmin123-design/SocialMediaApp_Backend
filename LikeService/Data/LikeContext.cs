using LikeService.Models;
using Microsoft.EntityFrameworkCore;

namespace LikeService.Data
{
    public class LikeContext : DbContext
    {
        public LikeContext(DbContextOptions<LikeContext> options) : base(options) { }

        public DbSet<Like> Likes { get; set; }
    }
}
