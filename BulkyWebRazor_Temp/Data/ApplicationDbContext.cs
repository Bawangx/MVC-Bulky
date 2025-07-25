using BulkyWebRazor_Temp.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyWebRazor_Temp.Data
{
    // DbContext adalah class utama untuk berinteraksi dengan database lewat Entity Framework Core
    public class ApplicationDbContext : DbContext
    {
        // Konstruktor yang menerima opsi konfigurasi DbContext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet mewakili tabel Category di database
        public DbSet<Category> Categories { get; set; }

        // Method ini digunakan untuk mengatur model dan melakukan seeding data awal
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Menambahkan data default ke tabel Categories (seeding)
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Sci-Fi", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 }
            );
        }
    }
}