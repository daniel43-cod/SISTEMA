using API_SISTEMA.data;
using API_SISTEMA.DTOs;
using API_SISTEMA.models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services
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
