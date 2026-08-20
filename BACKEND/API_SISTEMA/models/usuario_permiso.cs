namespace API_SISTEMA.models
{
    public class usuario_permiso
    {
        public int id_usuario_permiso {  get; set; }
        public int id_usuario { get; set; }
        public int id_permiso { get; set; }
        public bool permitido { get; set; }   

        public Usuario Usuario { get; set; }
        public Tabla_permiso tabla_permiso { get;  set; }
    }
}
