using APIs_v0._1.Models;
using APIs_v0._1.Services;
using Microsoft.AspNetCore.Mvc;

namespace APIs_v0._1.Controllers
{
    [ApiController]
    [Route("api/autos")]
    public class AutoController : ControllerBase
    {

        private readonly MongoDbService _mongoDbService;

        public AutoController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // GET: api/auto (Obtener todos los autos)
        [HttpGet]
        public async Task<ActionResult<List<Auto>>> Get()
        {
            var autos = await _mongoDbService.GetAllAutos();
            return Ok(autos);
        }

        // GET: api/auto/{id} (Obtener auto por ID)
        [HttpGet("{id}")]
        public async Task<ActionResult<Auto>> Get(string id)
        {
            var auto = await _mongoDbService.GetAutosById(id);
            if (auto == null) return NotFound("Auto no encontrado.");
            return Ok(auto);
        }

        // POST: api/auto (Agregar nuevo auto)
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Auto auto)
        {
            await _mongoDbService.AgregarAuto(auto);
            return CreatedAtAction(nameof(Get), new { id = auto.Id }, auto);
        }

        // PUT: api/auto/{id} (Actualizar auto)
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Auto auto)
        {
            var updated = await _mongoDbService.ActualizarAuto(id, auto);
            if (!updated) return NotFound("Auto no encontrado.");
            return NoContent();
        }

        // DELETE: api/auto/{id} (Eliminar auto)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _mongoDbService.EliminarAuto(id);
            if (!deleted) return NotFound("Auto no encontrado.");
            return NoContent();
        }

        [HttpGet("modelo/{modelo}")]
        public async Task<ActionResult<List<Auto>>> GetByModelo(string modelo)
        {
            var autos = await _mongoDbService.GetAutosByModelo(modelo);
            if (autos == null || autos.Count == 0) return NotFound("No se encontraron autos con ese modelo.");
            return Ok(autos);
        }

        [HttpGet("fabricante/{fabricante}")]
        public async Task<ActionResult<List<Auto>>> GetByFabricante(string fabricante)
        {
            var autos = await _mongoDbService.GetAutosByFabricante(fabricante);
            if (autos == null || autos.Count == 0) return NotFound("No se encontraron autos de ese fabricante.");
            return Ok(autos);
        }


    }
}
