using Microsoft.EntityFrameworkCore;
using APIs_v0._1.Models;
using JO3Motors_API.Models;

namespace APIs_v0._1.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Seguro> Seguros { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
    }
}
