using APIs_v0._1.Data;
using APIs_v0._1.Models;
using APIs_v0._1.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIs_v0._1.Controllers
{
    [ApiController]
    [Route("api/usuario")]
    public class UsuarioController:ControllerBase
    {
        private readonly AppDbContext context;
        public UsuarioController(AppDbContext context)
        {
            this.context = context;
        }
        //traer los usuarios
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await context.Usuarios.ToListAsync();
                return Ok(usuarios); // Devuelve 200 OK con la lista de usuarios
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}"); // Devuelve el error 500
            }

        }

        [HttpGet("{id:int}", Name = "ObtenerUsuarioPorId")]
        public async Task<ActionResult> GetusuariosId([FromRoute] int id)
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(x => x.IdUsuario == id);

            if (usuario is null)
            {
                return NotFound();
            }
            return Ok(usuario);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegistrarUsuario(Usuario usuario)
        {
            if (usuario == null)
            {
                return BadRequest("El usuario no puede ser nulo.");
            }

            if (!string.IsNullOrEmpty(usuario.Contrasenia))
            {
                usuario.Contrasenia = PasswordHelper.Hash(usuario.Contrasenia);
            }

            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            // Retornar 201 Created con la ubicación del nuevo recurso
            return CreatedAtRoute(("ObtenerUsuarioPorId"), new { id = usuario.IdUsuario }, usuario);
        }



        [HttpPut("{id:int}")]
        public async Task<ActionResult> ModificaUsuarioID([FromRoute] int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound($"El id= {id} no exitse");

            }
            if (string.IsNullOrEmpty(usuario.Correo) && string.IsNullOrEmpty(usuario.Contrasenia))
            {
                return BadRequest("Debe proporcionar al menos un valor válido para actualizar.");
            }
            if (!string.IsNullOrEmpty(usuario.Contrasenia))
            {
                usuario.Contrasenia = PasswordHelper.Hash(usuario.Contrasenia);
            }
            context.Update(usuario);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> EliminaUsuario([FromRoute] int id)
        {
            var usuarioEliminado = await context.Usuarios.Where(x => x.IdUsuario == id).ExecuteDeleteAsync();
            if (usuarioEliminado == 0)
            {
                return NotFound($"El usuario con el id:{id} no fue encontrado");
            }
            return Ok();
        }
    }
}
