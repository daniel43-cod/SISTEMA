namespace API_SISTEMA.services.CuentaCliente
{
    public sealed class CorreoClienteEnUsoException : InvalidOperationException
    {
        public CorreoClienteEnUsoException()
            : base("Este correo ya está en uso. Ingresa otro correo.")
        {
        }
    }
}
