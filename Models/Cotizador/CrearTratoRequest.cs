namespace CRMSistema.Models.Cotizador
{
    /// <summary>
    /// Payload para crear un nuevo trato desde el cotizador.
    /// </summary>
    public class CrearTratoRequest
    {
        public int prospecto_id { get; set; }
        public string nombre_trato { get; set; }
        public decimal importe { get; set; }
        public int fase_id { get; set; }
        public string fecha_inicio_cotizacion { get; set; }
        public string fecha_limite_cotizacion { get; set; }
    }
}
