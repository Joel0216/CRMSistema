namespace CRMSistema.Models.Cotizador
{
    /// <summary>
    /// Unidades del configurador de RME.
    /// </summary>
    public class UnidadRmeDto
    {
        public int Unidad_ID { get; set; }
        public string Nombre_Unidad { get; set; }
        public decimal Capacidad_Toneladas { get; set; }
        public decimal Costo_Unitario { get; set; }
    }
}
