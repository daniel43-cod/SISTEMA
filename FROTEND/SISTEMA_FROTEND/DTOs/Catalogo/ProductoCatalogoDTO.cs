using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Catalogo
{
    public class ProductoCatalogoDTO
    {
        public int id_producto { get; set; }

        public string nombre { get; set; } = string.Empty;

        public string? imagen { get; set; }

        public int stock { get; set; }

        public List<PresentacionCatalogoDTO> presentaciones { get; set; } = new();
    }
}
