using BookCollectorWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BookCollector
{
    public class AppDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        private readonly IConfiguration _config;

        public AppDbContext(DbContextOptions options,IConfiguration config) : base(options)
        {
            _config = config;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>(entity =>
            {
                entity.OwnsOne(e => e.ImageInfo, imageInfo =>
                {
                    imageInfo.Property(io => io.Bytes).HasColumnName("ImageBytes").IsRequired();
                    imageInfo.Property(io => io.Name).HasColumnName("ImageName");
                });

                entity.OwnsOne(e => e.BookDocumentInfo, bookDocInfo =>
                {
                    bookDocInfo.Property(io => io.Bytes).HasColumnName("BookDocumentBytes").IsRequired();
                    bookDocInfo.Property(io => io.Name).HasColumnName("BookDocumentName");
                });

            });
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(_config.GetConnectionString("DefaultConnection"));
        }
    }
}
