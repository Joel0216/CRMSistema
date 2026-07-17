namespace CRMSistema.Models.Cotizador
{
    /// <summary>
    /// Payload para enviar una cotización por correo.
    /// </summary>
    public class EnviarCotizacionRequest
    {
        public int prospecto_id { get; set; }
        public string email { get; set; }
        public string nombre { get; set; }
    }
}
