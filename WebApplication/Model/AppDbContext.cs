using Microsoft.EntityFrameworkCore;
using WebApplication.Model.Entities;

namespace WebApplication.Model
{
    public class AppDbContext : DbContext
    {
        public DbSet<UploadedFile> Files { get; set; }
        public DbSet<DataValue> Values { get; set; }
        public DbSet<AggregationResult> Results { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UploadedFile>()
                .HasMany(f => f.Values)
                .WithOne(v => v.File)
                .HasForeignKey(v => v.FileId)
                .OnDelete(DeleteBehavior.Cascade);
    
            modelBuilder.Entity<UploadedFile>()
                .HasOne(f => f.Result)
                .WithOne(r => r.File)
                .HasForeignKey<AggregationResult>(r => r.FileId)
                .OnDelete(DeleteBehavior.Cascade);
    
            modelBuilder.Entity<AggregationResult>()
                .HasIndex(r => r.FileId)
                .IsUnique();
        }
    }
}