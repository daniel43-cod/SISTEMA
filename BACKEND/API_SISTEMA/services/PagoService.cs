using API_SISTEMA.Data;
using API_SISTEMA.Dtos;
using API_SISTEMA.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services
{
    public class PagoService
    {

        private readonly SistemaDbContext _context;

        public PagoService(SistemaDbContext context)
        {
            _context = context;
        }

       

    }
}
