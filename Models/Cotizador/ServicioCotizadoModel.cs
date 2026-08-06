namespace CRMSistema.Models.Cotizador
{
    /// <summary>
    /// Servicio cotizado dentro de un trato.
    /// </summary>
    public class ServicioCotizadoModel
    {
        public int id { get; set; }
        public int trato_id { get; set; }
        public string tipo_residuo { get; set; }
        public string frecuencia { get; set; }
        public string periodicidad_pago { get; set; }
        public decimal? volumen_estimado { get; set; }
        public decimal? precio_unitario { get; set; }
        public string dias_asignados { get; set; }
        public decimal? porcentaje_adicional { get; set; }
        public decimal? porcentaje_descuento { get; set; }
        public string sucursal_id { get; set; }
        public string tipo_unidad { get; set; }
        public string tipo_cobro { get; set; }
        public int? recolectores { get; set; }
        public string turno { get; set; }
        public string fecha_unica { get; set; }
        public string ruta { get; set; }
        public decimal? recorrido_km { get; set; }
        public decimal? costo_tonelada { get; set; }
        public decimal? costo_disposicion { get; set; }
        public decimal? costo_renta_base { get; set; }
        public decimal? capacidad_toneladas { get; set; }
    }
}
