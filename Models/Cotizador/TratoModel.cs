using System;

namespace CRMSistema.Models.Cotizador
{
    /// <summary>
    /// Trato u oportunidad vinculada a un prospecto.
    /// </summary>
    public class TratoModel
    {
        public int Trato_ID { get; set; }
        public int Prospecto_ID { get; set; }
        public string Nombre_Trato { get; set; }
        public decimal? Importe { get; set; }
        public int? Fase_ID { get; set; }
        public bool? Promesa_Pago_Cobranza { get; set; }
        public DateTime? Fecha_Cierre_Estimada { get; set; }
        public DateTime? Fecha_Creacion { get; set; }

        public string Fecha_Cierre_Estimada_Str => Fecha_Cierre_Estimada.HasValue ? Fecha_Cierre_Estimada.Value.ToString("yyyy-MM-dd") : null;
        public string Fecha_Creacion_Str => Fecha_Creacion.HasValue ? Fecha_Creacion.Value.ToString("yyyy-MM-dd") : null;

        // Propiedades usadas en el request de creación
        public int prospecto_id { get; set; }
        public string nombre_trato { get; set; }
        public decimal importe { get; set; }
        public int fase_id { get; set; }
        public string fecha_limite_cotizacion { get; set; }
        public string fecha_inicio_cotizacion { get; set; }
    }
}
