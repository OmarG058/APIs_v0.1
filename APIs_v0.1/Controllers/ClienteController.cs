using APIs_v0._1.Data;
using APIs_v0._1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIs_v0._1.Controllers
{
    [ApiController]
    [Route("api/Cliente")]
    public class ClienteController:ControllerBase
    {
        private readonly AppDbContext context;
        public ClienteController(AppDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetClientes()
        {
            try
            {
                var clientes = await context.Clientes.ToListAsync();

                if (clientes != null && clientes.Any())
                {
                    return Ok(clientes);
                }
                else
                {
                    return NotFound("No se encontraron clientes.");
                }
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, $"Ocurrió un error interno en el servidor: {ex.Message}");
            }
        }

        [HttpGet("{id:int}", Name = "ObtenerCliente")]
        public async Task<ActionResult<Cliente>> GetClientesbyID([FromRoute] int id)
        {
            try
            {
                var cliente = await context.Clientes.FirstOrDefaultAsync(x => x.IdCliente == id);

                if (cliente == null)
                {
                    return NotFound($"No se han encontrado registros con el ID: {id}");
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, $"Ocurrió un error interno en el servidor: {ex.Message}");
            }
        }

        [HttpGet("buscar", Name = "BuscarClientes")]
        public async Task<ActionResult<IEnumerable<Cliente>>> BuscarClientes(
            [FromQuery] string? nombre,
            [FromQuery] string? apellidos)
        {
            try
            {
                var query = context.Clientes.AsQueryable();

                if (!string.IsNullOrEmpty(nombre))
                {
                    query = query.Where(x => x.Nombre.ToLower().Contains(nombre.ToLower()));
                }

                if (!string.IsNullOrEmpty(apellidos))
                {
                    query = query.Where(x => x.Apellidos.ToLower().Contains(apellidos.ToLower()));
                }

                var clientes = await query.ToListAsync();

                if (clientes == null || clientes.Count == 0)
                {
                    return NotFound("No se encontraron clientes con los criterios proporcionados.");
                }

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al buscar clientes: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validar formato de CURP (opcional pero recomendado)
                if (!System.Text.RegularExpressions.Regex.IsMatch(cliente.Curp, @"^[A-Z]{4}\d{6}[A-Z]{6}\d{2}$"))
                {
                    return BadRequest("CURP con formato inválido.");
                }

                // Verificar duplicados
                var clienteExistente = await context.Clientes
                    .AnyAsync(c => c.Curp == cliente.Curp || c.RFC == cliente.RFC);

                if (clienteExistente)
                {
                    return Conflict("Ya existe un cliente con el mismo CURP o RFC.");
                }

                context.Add(cliente);
                await context.SaveChangesAsync();

                return CreatedAtRoute("ObtenerCliente", new { id = cliente.IdCliente }, cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Cliente>> PutCliente([FromRoute] int id, Cliente cliente)
        {
            try
            {
                if (id != cliente.IdCliente)
                {
                    return BadRequest("El ID proporcionado no coincide con el del cliente.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var clienteExistente = await context.Clientes.FindAsync(id);
                if (clienteExistente == null)
                {
                    return NotFound($"No se encontró un cliente con el ID: {id}");
                }

                // Actualizar campos específicos
                clienteExistente.Nombre = cliente.Nombre;
                clienteExistente.Apellidos = cliente.Apellidos;
                clienteExistente.Telefono = cliente.Telefono;
                clienteExistente.Curp = cliente.Curp;
                clienteExistente.RFC = cliente.RFC;
                clienteExistente.IdUsuario = cliente.IdUsuario;
                clienteExistente.Colonia = cliente.Colonia;
                clienteExistente.Calle = cliente.Calle;
                clienteExistente.Ciudad = cliente.Ciudad;
                clienteExistente.Estado = cliente.Estado;
                clienteExistente.CodigoPostal = cliente.CodigoPostal;

                await context.SaveChangesAsync();

                return Ok("Cliente actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteCliente([FromRoute] int id)
        {
            try
            {
                var registroBorrado = await context.Clientes
                    .Where(x => x.IdCliente == id)
                    .ExecuteDeleteAsync();

                if (registroBorrado == 0)
                {
                    return NotFound($"No se encontró un cliente con el ID: {id}");
                }

                return Ok($"Cliente con ID {id} eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
