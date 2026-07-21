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
               var idUsuarioClaim =
    User.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
    ?? User.FindFirstValue("sub");

if (!int.TryParse(idUsuarioClaim, out int idUsuario))
{
    return Unauthorized(new
    {
        mensaje = "No se pudo identificar al usuario autenticado."
    });
}

                var venta = await _context.CrearVenta(ventaDto, idUsuario);

                return Ok(new
                {
                    mensaje = "Venta registrada correctamente",
                    id_venta = venta.id_ventas,
                    total = venta.total,
                    monto_pagado = venta.monto_pagado,
                    saldo_pendiente = venta.saldo_pendiente,
                    id_estado_venta = venta.id_estado_venta,
                    id_sesion_caja = venta.id_sesion_caja
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

            /*{
  "id_cliente": 0,
  "clienteNuevo": {
    "id_Cliente": 0,
    "nombre": "Juan",
    "apellido": "Pérez",
    "nit": "CF",
    "dpi": "1234567890101",
    "telefono": "55555555",
    "correo_electronico": "juan@example.com",
    "direccion": "Cobán, Alta Verapaz"
  },
  "id_usuario": 0,
  "monto_pagado": 100,
  "observacion_pago": "Venta de prueba",
  "origen": "Mostrador",
  "id_sesion_caja": 0,
  "pago": {
    "monto_pagado": 100
  },
  "detalles": [
    {
      "id_producto": 1054,
      "cantidad": 1,
      "descuento": 0,
      "id_producto_presentacion": 1
    }
  ]
}*/
        }

    }


}
