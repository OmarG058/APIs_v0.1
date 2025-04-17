using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIs_v0._1.Data;
using APIs_v0._1.Models;
using APIs_v0._1.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

                await _context.Cotizaciones.AddAsync(cotizacion);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(ObtenerCotizacionPorId), new { idCotizacion = cotizacion.IdCotizacion }, cotizacion);
            }
            catch (Exception ex)
            {

                return StatusCode(500,$"Error interno en el servidor:{ex.Message}");
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
                    Auto = auto.FirstOrDefault()
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // PUT: /api/cotizaciones/{idCotizacion}
        [HttpPut("{idCotizacion}")]
        public async Task<IActionResult> ActualizarCotizacion(int idCotizacion, [FromBody] Cotizacion actualizada)
        {
            try
            {
                var cotizacion = await _context.Cotizaciones.FindAsync(idCotizacion);

                if (cotizacion == null)
                    return NotFound("Cotización no encontrada.");

                if (actualizada == null)
                    return BadRequest("Datos inválidos.");

                // Actualiza los campos necesarios
                cotizacion.Fecha = actualizada.Fecha;
                cotizacion.IdCliente = actualizada.IdCliente;
                cotizacion.IdAuto = actualizada.IdAuto;
                cotizacion.IdSeguro = actualizada.IdSeguro;
                cotizacion.CostoTotal = actualizada.CostoTotal;

                _context.Cotizaciones.Update(cotizacion);
                await _context.SaveChangesAsync();

                return Ok(cotizacion);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
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
    }
}
