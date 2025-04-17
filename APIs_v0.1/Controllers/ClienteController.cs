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
            var clientes = await context.Clientes.ToListAsync();
            if (clientes != null)
            {
                return Ok(clientes);
            }
            return StatusCode(500, "Ocurrió un error interno en el servidor.");
        }

        [HttpGet("{id:int}", Name = "ObtenerCliente")]
        public async Task<ActionResult<Cliente>> GetClientesbyID([FromRoute] int id)
        {
            var cliente = await context.Clientes.FirstOrDefaultAsync(x => x.IdCliente == id);
            if (cliente == null)
            {
                return NotFound($"No se han Encontrado Registros con el id:{id}");
            }
            return Ok(cliente);

        }

        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            try
            {
                if (cliente != null)
                {
                    context.Add(cliente);
                    await context.SaveChangesAsync();
                    return CreatedAtRoute("ObtenerCliente", new { id = cliente.IdCliente });
                }
                else
                {
                    return BadRequest("El cliente no puede ser nulo.");
                }
            }
            catch (Exception ex)
            {
                // Aquí puedes loguear el error para obtener más información
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Cliente>> PutCliente([FromRoute] int id, Cliente cliente)
        {
            if (cliente != null)
            {
                context.Update(cliente);
                await context.SaveChangesAsync();
                return Ok(cliente + "Cleinte Actualizado");
            }
            if (id != cliente.IdCliente)
            {
                return NotFound($"El usuario con el {id} no exite");
            }
            return BadRequest("Datos Invalidos");
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Cliente>> DeleteCliente([FromRoute] int id)
        {
            var registroBorrado = await context.Clientes.Where(x => x.IdCliente == id).ExecuteDeleteAsync();
            if (registroBorrado == 0) { return NotFound($"El usuario con el {id} no exite"); };
            return Ok();

        }
    }
}
