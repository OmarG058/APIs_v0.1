using APIs_v0._1.Data;
using APIs_v0._1.Models;
using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JO3Motors_API.Models;

namespace APIs_v0._1.Services
{
    public class SqlServerServices
    {
        private readonly AppDbContext context;

        public SqlServerServices(AppDbContext context)
        {
            this.context = context;
        }
        //metodo para encontrar un cliente por nombre y apellidos
         
        public async Task<Cliente> GetClienteByNombre(string nombre, string apellidos)
        {
            try
            {
                var cliente = await context.Clientes.Where(c => c.Nombre.ToLower() == nombre.ToLower() && c.Apellidos.ToLower() == apellidos.ToLower()).FirstOrDefaultAsync();
                return cliente;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el cliente por nombre", ex);
            }
        }

        //cotizaciones
        public async Task<Cotizacion?> GetCotizacionById(int id)
        {
            return await context.Cotizaciones
                .FirstOrDefaultAsync(c => c.IdCotizacion == id);
        }

        public async Task<decimal?> GetCostoTotalById(int id)
        {
            return await context.Cotizaciones
                .Where(c => c.IdCotizacion == id)
                .Select(c => c.CostoTotal)
                .FirstOrDefaultAsync();
        }
        //seguros
        public async Task<Seguro?> GetSeguroById(int id)
        {
            return await context.Seguros
                .FirstOrDefaultAsync(s => s.IdSeguro == id);
        }
    }
}
