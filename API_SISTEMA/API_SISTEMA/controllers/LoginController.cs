using API_SISTEMA.DTOs;
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
    }
}
