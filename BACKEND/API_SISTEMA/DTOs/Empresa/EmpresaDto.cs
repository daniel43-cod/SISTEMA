namespace API_SISTEMA.Dtos.Empresa
{
    public class EmpresaDto
    {
        public int id_empresa { get; set; }
        public required string nombre_empresa { get; set; }
        public required string nit { get; set; }

    }
}
