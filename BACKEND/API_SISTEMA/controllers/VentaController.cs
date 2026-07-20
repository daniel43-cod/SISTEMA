using API_SISTEMA.data;
using API_SISTEMA.DTOs.Ventas;
using API_SISTEMA.models;
using API_SISTEMA.services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API_SISTEMA.controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {

        private readonly VentaService _context;

        public VentaController(VentaService service)
        {
            _context = service;
        }

        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpGet("listar")]
        public async Task<IActionResult> ListarVentas()
        {
            var listar = await _context.ListarVentes();
            return Ok(listar);
        }

        [Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] VentasDTOs ventaDto)
        {
            try
            {
                //se obtiene el id del usuario
                var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(idUsuario))
                    return Unauthorized();

                ventaDto.id_usuario = int.Parse(idUsuario); // ignora lo que mande el cliente, usa el del token

                var venta = await _context.CrearVenta(ventaDto);

                return Ok(new
                {
                    mensaje = "Venta registrada correctamente",
                    id_venta = venta.id_ventas,
                    total = venta.total,
                    monto_pagado = venta.monto_pagado,
                    saldo_pendiente = venta.saldo_pendiente,
                    id_estado_venta = venta.id_estado_venta
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
