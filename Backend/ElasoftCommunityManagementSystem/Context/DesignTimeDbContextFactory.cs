using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ElasoftCommunityManagementSystem.Context
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Çalışma dizini
            var basePath = Directory.GetCurrentDirectory();

            // appsettings.json dosyasını yükle
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .Build();

            // Bağlantı cümlesini al
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // DbContext options'ı oluştur
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // AppDbContext döndür
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
