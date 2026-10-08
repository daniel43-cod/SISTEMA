namespace API_SISTEMA.Services.CuentaCliente
{
    public sealed class CorreoClienteEnUsoException : InvalidOperationException
    {
        public CorreoClienteEnUsoException()
            : base("Este correo ya está en uso. Ingresa otro correo.")
        {
        }
    }
}
