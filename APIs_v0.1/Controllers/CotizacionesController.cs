using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIs_v0._1.Data;
using APIs_v0._1.Models;
using APIs_v0._1.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JO3Motors_API.Models;


namespace JO3Motors_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CotizacionesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MongoDbService _mongoService;

        public CotizacionesController(AppDbContext context, MongoDbService mongoService)
        {
            _context = context;
            _mongoService = mongoService;
        }

        // POST: /api/cotizaciones
        [HttpPost]
        public async Task<IActionResult> CrearCotizacion([FromBody] Cotizacion cotizacion)
        {
            try
            {
                if (cotizacion == null)
                    return BadRequest("Datos inválidos.");

                // Obtener precio del seguro
                decimal costoSeguro = 0;
                if (cotizacion.IdSeguro.HasValue && cotizacion.IdSeguro.Value != 0) //El si no selecciona seguro se guarda como 0 el cual biene del front
                {
                    var seguro = await _context.Seguros.FindAsync(cotizacion.IdSeguro.Value);
                    if (seguro == null)
                        return BadRequest("Seguro no encontrado.");
                    costoSeguro = seguro.Costo;
                }

                // Obtener precio del auto
                var auto = await _mongoService.GetAutosById(cotizacion.IdAuto);
                if (auto == null)
                    return BadRequest("Auto no encontrado.");


                // Obtener accesorios si se proporcionaron
                var accesorioIds = new List<string>();
                if (!string.IsNullOrEmpty(cotizacion.IdAsiento)) accesorioIds.Add(cotizacion.IdAsiento);
                if (!string.IsNullOrEmpty(cotizacion.IdColor)) accesorioIds.Add(cotizacion.IdColor);
                if (!string.IsNullOrEmpty(cotizacion.IdRin)) accesorioIds.Add(cotizacion.IdRin);

                var accesorios = await _mongoService.GetAccesoriosByIds(accesorioIds);
                var costoAccesorios = accesorios.Sum(a => a.Costo);

                // Calcular costo total
                decimal costoTotal = costoSeguro + auto.Costo + costoAccesorios;

                cotizacion.CostoTotal = costoTotal;

                // Si es crédito, aplicar intereses según el plazo
                if (cotizacion.TipoPago.ToLower() == "credito" && cotizacion.PlazoMeses.HasValue)
                {
                    if (!cotizacion.PlazoMeses.HasValue || (cotizacion.PlazoMeses != 12 && cotizacion.PlazoMeses != 24 && cotizacion.PlazoMeses != 36))
                    {
                        return BadRequest("Plazo de meses inválido. Debe ser 12, 24 o 36 para crédito.");
                    }

                    decimal porcentajeInteres = cotizacion.PlazoMeses.Value switch
                    {
                        12 => 0.10m,
                        24 => 0.20m,
                        36 => 0.30m,
                        _ => 0.0m
                    };

                    costoTotal += costoTotal * porcentajeInteres;
                    cotizacion.Mensualidad = Math.Round(costoTotal / cotizacion.PlazoMeses.Value, 2);
                }
                if(cotizacion.TipoPago.ToLower() == "contado")
                {
                    cotizacion.PlazoMeses = 0;
                    cotizacion.Mensualidad = 0;
                    cotizacion.IdSeguro = null;
                }

                cotizacion.CostoTotal = costoTotal;
                cotizacion.Fecha = DateTime.UtcNow;

                // Guardar cotización
                await _context.Cotizaciones.AddAsync(cotizacion);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(ObtenerCotizacionPorId), new { idCotizacion = cotizacion.IdCotizacion }, cotizacion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno en el servidor: {ex.Message} | {ex.InnerException?.Message}");
            }
        }

        // GET: /api/cotizaciones
        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {

            try
            {
                var cotizaciones = await _context.Cotizaciones.ToListAsync();
                return Ok(cotizaciones);

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
       
        }

        // GET: /api/cotizaciones/clientes/{idCliente}
        [HttpGet("clientes/{idCliente}")]
        public async Task<IActionResult> ObtenerPorCliente(int idCliente)
        {
            try
            {
                var cotizaciones = await _context.Cotizaciones
                .Where(c => c.IdCliente == idCliente)
                .ToListAsync();

                if (cotizaciones == null || cotizaciones.Count == 0)
                    return NotFound("No se encontraron cotizaciones para el cliente.");

                return Ok(cotizaciones);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error interno del servidor: {ex.Message}");    
            }
        }

        // GET: /api/cotizaciones/{idCotizacion}
        [HttpGet("{idCotizacion}")]
        public async Task<IActionResult> ObtenerCotizacionPorId(int idCotizacion)
        {
            try
            {
                var cotizacion = await _context.Cotizaciones.FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

                if (cotizacion == null)
                    return NotFound("Cotización no encontrada.");

                var cliente = await _context.Clientes.FindAsync(cotizacion.IdCliente);
                var seguro = await _context.Seguros.FindAsync(cotizacion.IdSeguro);
                var auto = await _mongoService.GetAutosById(cotizacion.IdAuto);

                var resultado = new
                {
                    Cotizacion = cotizacion,
                    Cliente = cliente,
                    Seguro = seguro,
                    Auto = auto //FirstOrDefault()
                    //Quite el FirstOrDefault() porque GetAutosById ya lo tiene por id exacto, no es necesario el primero de la lista
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // PUT: /api/cotizaciones/{idCotizacion}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCotizacion(int id, [FromBody] Cotizacion cotizacion)
        {
            try
            {
                if (cotizacion == null || id != cotizacion.IdCotizacion)
                    return BadRequest("Datos inválidos o ID inconsistente.");

                var cotizacionExistente = await _context.Cotizaciones.FindAsync(id);
                if (cotizacionExistente == null)
                    return NotFound("Cotización no encontrada.");

                // Obtener precio del seguro
                decimal costoSeguro = 0;
                if (cotizacion.IdSeguro.HasValue)
                {
                    var seguro = await _context.Seguros.FindAsync(cotizacion.IdSeguro.Value);
                    if (seguro == null) return BadRequest("Seguro no encontrado.");
                    costoSeguro = seguro.Costo;
                }

                // Obtener precio del auto
                var auto = await _mongoService.GetAutosById(cotizacion.IdAuto);
                if (auto == null) return BadRequest("Auto no encontrado.");

                // Accesorios
                var accesorioIds = new List<string>();
                if (!string.IsNullOrEmpty(cotizacion.IdAsiento)) accesorioIds.Add(cotizacion.IdAsiento);
                if (!string.IsNullOrEmpty(cotizacion.IdColor)) accesorioIds.Add(cotizacion.IdColor);
                if (!string.IsNullOrEmpty(cotizacion.IdRin)) accesorioIds.Add(cotizacion.IdRin);

                var accesorios = await _mongoService.GetAccesoriosByIds(accesorioIds);
                var costoAccesorios = accesorios.Sum(a => a.Costo);

                // Calcular total
                decimal costoTotal = costoSeguro + auto.Costo + costoAccesorios;

                if (cotizacion.TipoPago.ToLower() == "credito" && cotizacion.PlazoMeses.HasValue)
                {
                    if (cotizacion.PlazoMeses.Value != 12 && cotizacion.PlazoMeses.Value != 24 && cotizacion.PlazoMeses.Value != 36)
                        return BadRequest("Plazo inválido. Solo se permiten 12, 24 o 36 meses.");

                    decimal porcentajeInteres = cotizacion.PlazoMeses.Value switch
                    {
                        12 => 0.10m,
                        24 => 0.20m,
                        36 => 0.30m,
                        _ => 0.0m
                    };

                    costoTotal += costoTotal * porcentajeInteres;
                    cotizacion.Mensualidad = Math.Round(costoTotal / cotizacion.PlazoMeses.Value, 2);
                }
                else
                {
                    cotizacion.PlazoMeses = 0;
                    cotizacion.Mensualidad = 0;
                    cotizacion.IdSeguro = null;
                }

                // Actualizar campos
                cotizacionExistente.IdSeguro = cotizacion.IdSeguro;
                cotizacionExistente.IdAuto = cotizacion.IdAuto;
                cotizacionExistente.IdCliente = cotizacion.IdCliente;
                cotizacionExistente.IdAsiento = cotizacion.IdAsiento;
                cotizacionExistente.IdColor = cotizacion.IdColor;
                cotizacionExistente.IdRin = cotizacion.IdRin;
                cotizacionExistente.TipoPago = cotizacion.TipoPago;
                cotizacionExistente.PlazoMeses = cotizacion.PlazoMeses;
                cotizacionExistente.Mensualidad = cotizacion.Mensualidad;
                cotizacionExistente.CostoTotal = costoTotal;
                cotizacionExistente.Fecha = DateTime.UtcNow;

                _context.Cotizaciones.Update(cotizacionExistente);
                await _context.SaveChangesAsync();

                return Ok(cotizacionExistente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message} | {ex.InnerException?.Message}");
            }
        }

        // DELETE: /api/cotizaciones/{idCotizacion}
        [HttpDelete("{idCotizacion}")]
        public async Task<IActionResult> EliminarCotizacion(int idCotizacion)
        {
            var cotizacion = await _context.Cotizaciones.FindAsync(idCotizacion);

            if (cotizacion == null)
                return NotFound("Cotización no encontrada.");

            _context.Cotizaciones.Remove(cotizacion);
            await _context.SaveChangesAsync();

            return Ok("Cotización eliminada exitosamente.");
        }

        // GET: /api/cotizaciones/buscar-cliente?nombre=jorge valdez
        [HttpGet("buscar-cliente")]
        public async Task<IActionResult> BuscarPorNombreCliente([FromQuery] string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                    return BadRequest("Debes proporcionar un nombre o apellido.");

                // Convertir el nombre en palabras individuales (por si mandan "jorge valdez")
                var palabras = nombre.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);

                // Buscar clientes donde alguna palabra coincida con el nombre o apellidos
                var clientes = await _context.Clientes
                    .Where(c => palabras.Any(p =>
                        c.Nombre.ToLower().Contains(p) ||
                        c.Apellidos.ToLower().Contains(p)))
                    .ToListAsync();

                if (clientes == null || clientes.Count == 0)
                    return NotFound("No se encontraron clientes con esos datos.");

                var idsClientes = clientes.Select(c => c.IdCliente).ToList();

                var cotizaciones = await _context.Cotizaciones
                    .Where(c => idsClientes.Contains(c.IdCliente))
                    .ToListAsync();

                return Ok(cotizaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
