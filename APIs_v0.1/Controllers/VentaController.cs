using APIs_v0._1.Data;
using APIs_v0._1.Helper;
using APIs_v0._1.Models;
using APIs_v0._1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;


namespace APIs_v0._1.Controllers
{
    [ApiController]
    [Route("api/Ventas")]
    public class VentaController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly MongoDbService mongoServices;
        private readonly SqlServerServices sqlServices;

        public VentaController(AppDbContext Context, MongoDbService MongoServices, SqlServerServices SqlServices)
        {
            this.context = Context;
            this.mongoServices = MongoServices;
            this.sqlServices = SqlServices;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerVentas()
        {
            try
            {
                var ventas = await mongoServices.GetVentas();
                if (ventas != null)
                {
                    return Ok(ventas);
                }
                return NotFound("No se encontraron ventas.");
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }


        }
        [HttpGet("{id}", Name = "ObtenerVentaPorId")]
        public async Task<IActionResult> ObtenerVentaId([FromRoute] string id)
        {
            try
            {
                var venta = await mongoServices.GetVentasById(id);
                if (venta != null)
                    return Ok(venta);
                return NotFound($"No se encontró la venta con el ID: {id}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("ventas-por-cliente")]
        public async Task<IActionResult> ObtenerVentasPorCliente([FromQuery] string nombre, [FromQuery] string apellidos)
        {
            try
            {
                var cliente = await sqlServices.GetClienteByNombre(nombre, apellidos);

                if (cliente == null)
                    return NotFound("No se encontró el cliente.");

                if (cliente.IdCliente == null)
                    return BadRequest("El cliente no tiene un ID válido.");

                var ventas = await mongoServices.GetVentasByCliente(cliente.IdCliente.Value);

                if (ventas != null && ventas.Any())
                    return Ok(ventas);

                return NotFound("No se encontraron ventas para este cliente.");
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpGet("ventas-por-fecha")]
        public async Task<IActionResult> ObtenerVentasPorFecha([FromQuery] DateTime fecha)
        {
            try
            {
                var ventas = await mongoServices.GetVentasByFecha(fecha);

                if (ventas != null && ventas.Any())
                    return Ok(ventas);

                return NotFound("No se encontraron ventas para la fecha especificada.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }  

        [HttpPost]
        public async Task<IActionResult> PostVenta([FromBody] Venta venta)
        {
            try
            {
                if (venta == null)
                {
                    return BadRequest("La venta no puede ser nula.");
                }  

                // Validación si no contrató seguro
                if (venta.VentaSeguro != null && !venta.VentaSeguro.Contratado)
                {
                    venta.VentaSeguro.TipoSeguro = null;
                    venta.VentaSeguro.TipoPago = null;
                    venta.VentaSeguro.PrecioTotal = 0;
                    venta.VentaSeguro.SaldoPendiente = 0;
                    venta.VentaSeguro.PagosSeguro = new List<Pago>();
                }

                await mongoServices.AgregarVenta(venta);
                return CreatedAtRoute(("ObtenerVentaPorId"), new { id = venta.Id }, venta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenta([FromRoute] string id, [FromBody] Venta venta)
        {
            try
            {
                var ventaExistente = await mongoServices.GetVentasById(id);
                if (ventaExistente == null)
                {
                    return NotFound($"No se encontró ninguna venta con el ID: {id}.");
                }

                if (id != venta.Id)
                {
                    return BadRequest($"El ID de la ruta ({id}) no coincide con el ID de la venta ({venta.Id}).");
                }

                await mongoServices.ActualizarVenta(id, venta);

                return Ok("Venta actualizada correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenta([FromRoute] string id)
        {
            try
            {
                var resultado = await mongoServices.EliminarVenta(id);
                if (resultado.DeletedCount == 0)
                {
                    return NotFound($"No se encontró una venta con el id: {id}");
                }

                return Ok($"Venta con id: {id} eliminada correctamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}
