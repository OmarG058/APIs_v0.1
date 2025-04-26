using APIs_v0._1.Data;
using APIs_v0._1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using JO3Motors_API.Models;

namespace J30_API_SEGURO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SegurosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SegurosController(AppDbContext context)
        {
            _context = context;
        }

        // TODOS LOS SEGUROS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Seguro>>> GetSeguros()
        {
            return await _context.Seguros.ToListAsync();
        }

        // BUSCAR SEGURO POR ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Seguro>> GetSeguro(int id)
        {
            var seguro = await _context.Seguros.FindAsync(id);

            if (seguro == null)
            {
                return NotFound();
            }

            return seguro;
        }

        // ACTUALIZAR SEGURO POR ID
        [HttpPut("modificar/{id}")]
        public async Task<IActionResult> PutSeguro(int id, Seguro seguro)
        {
            if (id != seguro.IdSeguro)
            {
                return BadRequest();
            }

            _context.Entry(seguro).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeguroExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // AÑADIR UN NUEVO SEGURO
        [HttpPost("agregar")]
        public async Task<ActionResult<Seguro>> PostSeguro(Seguro seguro)
        {
            _context.Seguros.Add(seguro);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSeguro", new { id = seguro.IdSeguro }, seguro);
        }

        // ELIMINAR SEGURO POR ID
        [HttpDelete("/api/Seguros/eliminar/{id}")]
        public async Task<IActionResult> DeleteSeguro(int id)
        {
            var seguro = await _context.Seguros.FindAsync(id);
            if (seguro == null)
            {
                return NotFound();
            }

            _context.Seguros.Remove(seguro);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SeguroExists(int id)
        {
            return _context.Seguros.Any(e => e.IdSeguro == id);
        }
    }
}