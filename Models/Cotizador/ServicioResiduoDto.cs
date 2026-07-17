namespace CRMSistema.Models.Cotizador
{
    /// <summary>
    /// Catálogo de residuos disponibles para cotizar.
    /// </summary>
    public class ServicioResiduoDto
    {
        public string codigo_control { get; set; }
        public string codigo_sana { get; set; }
        public string tipo { get; set; }
        public string descripcion { get; set; }
        public decimal precio { get; set; }
        public string unidad_medida { get; set; }
    }
}
