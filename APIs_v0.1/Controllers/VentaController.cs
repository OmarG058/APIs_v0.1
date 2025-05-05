using APIs_v0._1.Data;
using APIs_v0._1.Helper;
using APIs_v0._1.Models;
using APIs_v0._1.Services;
using JO3Motors_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System.Runtime.InteropServices;


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
                var venta = await mongoServices.GetVentaById(id);
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

        //Agregar una venta en base a la cotización
        [HttpPost("nuevo")]
        public async Task<IActionResult> CrearVenta([FromBody] CrearVentaRequest request)
        {
            // Obtener cotización
            var cotizacion = await sqlServices.GetCotizacionById(request.IdCotizacion);
            if (cotizacion == null)
                return NotFound("Cotización no encontrada.");

            // Obtener auto (de MongoDB)
            var auto = await mongoServices.GetAutosById(cotizacion.IdAuto);
            if (auto == null)
                return NotFound("Auto no encontrado.");

            // Calcular costos de accesorios si hay

            // Calcular total
            var costoTotal = await sqlServices.GetCostoTotalById(cotizacion.IdCotizacion);

            // Armar VentaAuto
            var ventaAuto = new VentaAuto
            {
                PrecioTotal = (decimal)costoTotal,
                TipoPago = cotizacion.TipoPago,
                SaldoPendiente = (decimal)costoTotal,
                PagosAuto = new List<Pago>()
            };

            // Armar VentaSeguro
            VentaSeguro ventaSeguro = new VentaSeguro
            {

            };

            if (cotizacion.IdSeguro.HasValue)
            {
                var seguro = await sqlServices.GetSeguroById(cotizacion.IdSeguro.Value);
                if (seguro == null)
                    return NotFound("Seguro no encontrado.");

                var fechaInicio = DateTime.UtcNow;

                ventaSeguro = new VentaSeguro
                {

                    Estado = "Activo",
                    PrecioTotal = seguro.Costo,
                    SaldoPendiente = 0,
                    FechaContratacion = fechaInicio,
                    FechaFinalizacion = fechaInicio.AddMonths(seguro.Duracion),
                    PagosSeguro = new List<Pago>() // Lista vacía, pagos se registran aparte
                };
            }
            if (cotizacion.IdSeguro is null)
            {
                var fecha = DateTime.UtcNow;



                ventaSeguro = new VentaSeguro
                {

                    Estado = "Desactivado",
                    FechaContratacion = fecha,
                    FechaFinalizacion = fecha,
                    PagosSeguro = new List<Pago>()
                };
            }

            // Crear venta
            var venta = new Venta
            {
                IdCotizacion = cotizacion.IdCotizacion,
                IdAuto = cotizacion.IdAuto,
                IdCliente = cotizacion.IdCliente,
                IdSeguro = (int?)cotizacion.IdSeguro,
                Fecha = DateTime.UtcNow,
                VentaAuto = ventaAuto,
                VentaSeguro = ventaSeguro
            };

            await mongoServices.AgregarVenta(venta);

            return Ok("Venta creada correctamente.");
        }


        //Agregar un pago a la venta
        [HttpPost("/pago-auto")]
        public async Task<IActionResult> AgregarPagoAuto([FromBody] AgregarPagoAutoRequest request)
        {
            // Obtener la venta
            var venta = await mongoServices.GetVentaById(request.IdVenta);
            if (venta == null)
                return NotFound("Venta no encontrada.");

            // Crear el nuevo pago
            var nuevoPago = new Pago
            {
                Fecha = DateTime.UtcNow,
                Monto = request.Monto,
                Metodo = request.Metodo
            };

            // Agregar el pago y actualizar el saldo
            venta.VentaAuto.PagosAuto.Add(nuevoPago);
            venta.VentaAuto.SaldoPendiente -= request.Monto;


            if (venta.VentaAuto.SaldoPendiente < 0)
                venta.VentaAuto.SaldoPendiente = 0; // Protección contra valores negativos

            // Guardar cambios
            await mongoServices.ActualizarVenta(request.IdVenta, venta);

            return Ok("Pago registrado correctamente.");
        }

        [HttpPost("reactivar-seguro")]
        public async Task<IActionResult> ReactivarSeguro([FromBody] ActivarSeguroRequest request)
        {
            var venta = await mongoServices.GetVentaById(request.IdVenta);
            if (venta == null)
                return NotFound("Venta no encontrada.");

            var seguro = await sqlServices.GetSeguroById(request.IdSeguro);
            if (seguro == null)
                return NotFound("Nuevo seguro no encontrado.");

            var fechaInicio = DateTime.UtcNow;

            var nuevoVentaSeguro = new VentaSeguro
            {
                Estado = "Activo",
                FechaContratacion = fechaInicio,
                FechaFinalizacion = fechaInicio.AddMonths(seguro.Duracion),
                PrecioTotal = seguro.Costo,
                SaldoPendiente = seguro.Costo - request.Monto,
                PagosSeguro = new List<Pago>
        {
            new Pago
            {
                Fecha = fechaInicio,
                Metodo = request.MetodoPago,
                Monto = request.Monto
            }
        }
            };

            // Actualizar el ID del seguro en la venta
            venta.IdSeguro = request.IdSeguro;
            venta.VentaSeguro = nuevoVentaSeguro;

            await mongoServices.ActualizarVenta(venta.Id, venta);

            return Ok("Seguro reactivado correctamente con nuevo seguro.");
        }

        //Agregar un pago al seguro
        [HttpPost("{id}/pago-seguro")]
        public async Task<IActionResult> AgregarPagoSeguro(string id, [FromBody] PagoRequestSeguro request)
        {
            var venta = await mongoServices.GetVentaById(id);
            if (venta == null)
                return NotFound("Venta no encontrada.");

            if (venta.VentaSeguro.Estado != "Activo")
                return BadRequest("El seguro no está activo.");

            var nuevoPago = new Pago
            {
                Fecha = DateTime.UtcNow,
                Metodo = request.Metodo,
                Monto = request.Monto
            };

            venta.VentaSeguro.PagosSeguro.Add(nuevoPago);
            venta.VentaSeguro.SaldoPendiente -= request.Monto;

            if (venta.VentaSeguro.SaldoPendiente <= 0)
            {
                venta.VentaSeguro.SaldoPendiente = 0;
                // opcional: podrías marcarlo como "Pagado" si lo deseas
            }

            await mongoServices.ActualizarVenta(id, venta);
            return Ok("Pago al seguro registrado correctamente.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenta([FromRoute] string id, [FromBody] Venta venta)
        {
            try
            {
                var ventaExistente = await mongoServices.GetVentaById(id);
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
