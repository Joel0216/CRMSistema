namespace CRMSistema.Models.Cotizador
{
    /// <summary>
    /// Borrador de cotización asociado a un prospecto.
    /// </summary>
    public class BorradorDto
    {
        public int Borrador_ID { get; set; }
        public int Prospecto_ID { get; set; }
        public string Datos_Borrador { get; set; }
        public string Fecha_Creacion { get; set; }
    }
}
