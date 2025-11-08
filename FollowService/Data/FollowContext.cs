using FollowService.Models;
using Microsoft.EntityFrameworkCore;

namespace FollowService.Data
{
    public class FollowContext : DbContext
    {
        public FollowContext(DbContextOptions<FollowContext> options) : base(options) { }

        public DbSet<Follow> Follows { get; set; }

    }
}
