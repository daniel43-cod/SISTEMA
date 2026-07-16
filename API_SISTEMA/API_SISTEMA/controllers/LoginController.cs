using API_SISTEMA.DTOs.Login;
using API_SISTEMA.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly LoginService _Service;

        public LoginController(LoginService service)
        {
            _Service = service;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTOs dto)
        {
            var respuesta = await _Service.Login(dto);

            if (respuesta == null)
                return Unauthorized("Usuario o contraseña incorrectos.");

            return Ok(respuesta);
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Login(CrearCuentaDTOs dto)
        {
            try
            {
                await _Service.CrearUsuario(dto);

                return Ok(new
                {
                    mensaje = "Usuario creado correctamente."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message,
                    detalle = ex.ToString()
                });
            }
        }
    }
    
}
