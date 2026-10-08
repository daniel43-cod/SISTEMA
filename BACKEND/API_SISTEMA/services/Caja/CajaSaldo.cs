using API_SISTEMA.Data;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Caja;

public sealed record CajaSaldo(decimal Entradas, decimal Salidas, decimal Esperado, int Cantidad);
