using ImageService.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Data
{
    public class ImageDbContext : DbContext
    {
        public ImageDbContext(DbContextOptions<ImageDbContext> options) : base(options) { }
        public DbSet<Image> Images { get; set; }
    }
}
