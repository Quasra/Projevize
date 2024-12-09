using Microsoft.EntityFrameworkCore;

namespace Internet_1.Models
{
    public static class SeedData
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category() { Id = 1, Name = "Categori 1", IsActive = true },
                new Category() { Id = 2, Name = "Categori 1", IsActive = true },
                new Category() { Id = 3, Name = "Categori 2", IsActive = true }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product() { Id = 1, Name = "Kalem", Path = @"C:\VICTUS\Your\Directory\Kalem", Type = "Kalem açıklama", IsActive = true  },
                new Product() { Id = 2, Name = "Defter", Path = @"C:\VICTUS\Your\Directory\Defter", Type = "Defter açıklama", IsActive = true },
                new Product() { Id = 3, Name = "Silgi", Path = @"C:\VICTUS\Your\Directory\Silgi", Type = "Silgi açıklama", IsActive = true },
                new Product() { Id = 4, Name = "Kitap", Path = @"C:\VICTUS\Your\Directory\Kitap", Type = "Kitap açıklama", IsActive = true }
            );
        }
    }
}
