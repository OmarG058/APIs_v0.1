using APIs_v0._1.Data;
using APIs_v0._1.Models;
using APIs_v0._1.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
                return Ok(usuarios); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}"); 
            }

        }

        [HttpGet("{id:int}", Name = "ObtenerUsuarioPorId")]
        public async Task<ActionResult> GetusuariosId([FromRoute] int id)
        {
            try
            {
                var usuario = await context.Usuarios.FirstOrDefaultAsync(x => x.IdUsuario == id);

                if (usuario is null)
                {
                    return NotFound($"No se encontró el usuario con el ID: {id}");
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegistrarUsuario(Usuario usuario)
        {
            if (usuario == null)
            {
                return BadRequest("El usuario no puede ser nulo.");
            }

            if (string.IsNullOrWhiteSpace(usuario.Contrasenia))
            {
                return BadRequest("La contraseña no puede estar vacía.");
            }


            if (string.IsNullOrEmpty(usuario.Correo) || !new EmailAddressAttribute().IsValid(usuario.Correo))
            {
                return BadRequest("El correo electrónico es inválido.");
            }

            var usuarioExistente = await context.Usuarios
                .FirstOrDefaultAsync(x => x.Correo == usuario.Correo);

            if (usuarioExistente != null)
            {
                return BadRequest("Ya existe un usuario registrado con este correo electrónico.");
            }

            if (usuario.FechaRegistro == DateTime.MinValue)
            {
                usuario.FechaRegistro = DateTime.UtcNow;
            }

            var tipoUsuarioValido = await context.TiposUsuarios.FirstOrDefaultAsync(t => t.IdTipoUsuario == usuario.IdTipo);

            if (tipoUsuarioValido == null)
            {
                return BadRequest("El tipo de usuario no es válido.");
            }

            usuario.Contrasenia = PasswordHelper.Hash(usuario.Contrasenia);

            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

           
            return CreatedAtRoute("ObtenerUsuarioPorId", new { id = usuario.IdUsuario }, usuario);
        }



        [HttpPut("{id:int}")]
        public async Task<ActionResult> ModificaUsuarioID([FromRoute] int id, Usuario usuario)
        {
            try
            {
                // Verificar si el usuario existe
                var usuarioExistente = await context.Usuarios.FirstOrDefaultAsync(x => x.IdUsuario == id);
                if (usuarioExistente == null)
                {
                    return NotFound($"El usuario con el id={id} no existe.");
                }

                // Validar que el id proporcionado sea el mismo que el de la base de datos
                if (id != usuario.IdUsuario)
                {
                    return NotFound($"El id={id} no existe.");
                }

                // Validar que al menos uno de los campos correo o contraseña se haya proporcionado
                if (string.IsNullOrEmpty(usuario.Correo) && string.IsNullOrEmpty(usuario.Contrasenia))
                {
                    return BadRequest("Debe proporcionar al menos un valor válido para actualizar.");
                }

                // Validar el formato del correo
                if (!string.IsNullOrEmpty(usuario.Correo) && !new EmailAddressAttribute().IsValid(usuario.Correo))
                {
                    return BadRequest("El correo electrónico es inválido.");
                }


                // Si la contraseña ha sido proporcionada, la encriptamos
                if (!string.IsNullOrEmpty(usuario.Contrasenia))
                {
                    usuario.Contrasenia = PasswordHelper.Hash(usuario.Contrasenia);
                }

                // Si los datos no han cambiado, no procesamos la actualización
                if (usuarioExistente.Correo == usuario.Correo && usuarioExistente.Contrasenia == usuario.Contrasenia)
                {
                    return BadRequest("No se realizaron cambios en los datos proporcionados.");
                }

                // Actualizar el usuario
                context.Update(usuario);
                await context.SaveChangesAsync();

                // Retornar la respuesta
                return Ok("Usuario actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error interno: {ex.Message}");
            }
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
