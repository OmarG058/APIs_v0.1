using APIs_v0._1.Data;
using APIs_v0._1.Helper;
using APIs_v0._1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APIs_v0._1.Services;

namespace APIs_v0._1.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController: ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IConfiguration config;
        private readonly MongoDbService mongoServices;

        public AuthController(AppDbContext context, IConfiguration config,MongoDbService mongoServices)
        {
            this.context = context;
            this.config = config;
            this.mongoServices = mongoServices;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UsuarioLoginRequest request)
        {
            var user = context.Usuarios.SingleOrDefault(u => u.Correo == request.email);

            if (user == null || !PasswordHelper.Validate(request.password,user.Contrasenia))
                return BadRequest("los datos no son correctos");
                  
            var token = GenerarToken(user);

            //En mi caso si es admin el que se logue retornamos clientes("tabla usuarios en SQL") 
            if (user.IdTipo == 1)
            {
                var clientes =  context.Usuarios.Where(u => u.IdTipo.ToString() == "2").ToList();
                return Ok(new { token, tipo = user.IdTipo, clientes });
            }
            if (user.IdTipo == 2)
            {
                var Autos = await mongoServices.GetAllAutos();
                return Ok(new { token, tipo = user.IdTipo, Autos });
            }


            return BadRequest("Rol no reconcido");
        }


        private object GenerarToken(Usuario user)
        {
            var key = Encoding.ASCII.GetBytes(config["JwtSettings:SecretKey"]);

            var tokendescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Email, user.Correo.ToString()),
                    new Claim(ClaimTypes.Role, user.IdTipo.ToString())

                }),

                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokendescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
