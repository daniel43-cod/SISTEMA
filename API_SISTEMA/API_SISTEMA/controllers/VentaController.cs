using API_SISTEMA.data;
using API_SISTEMA.DTOs;
using API_SISTEMA.models;
using API_SISTEMA.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API_SISTEMA.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {

        private readonly VentaService _context;

        public VentaController(VentaService service)
        {
            _context = service;
        }

        [HttpGet]
        public async Task<IActionResult> ListarVentas()
        {
            var listar = await _context.ListarVentes();
            return Ok(listar);
        }


        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] VentasDTOs ventaDto)
        {
            try
            {
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
