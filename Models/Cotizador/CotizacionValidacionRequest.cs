namespace CRMSistema.Models.Cotizador
{
    /// <summary>
    /// Request para enviar o gestionar una validación de cotización.
    /// </summary>
    public class CotizacionValidacionRequest
    {
        public int prospecto_id { get; set; }
        public int? borrador_id { get; set; }
        public int? validacion_id { get; set; }
        public string datos { get; set; }
        public string motivo { get; set; }
    }
}
