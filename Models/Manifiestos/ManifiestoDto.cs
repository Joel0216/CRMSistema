using System;

namespace CRMSistema.Models.Manifiestos
{
    /// <summary>
    /// Placeholder para modelos de manifiestos.
    /// </summary>
    public class ManifiestoDto
    {
        public int Manifiesto_ID { get; set; }
        public string Folio { get; set; }
        public int Prospecto_ID { get; set; }
        public int Trato_ID { get; set; }
        public string Generador { get; set; }
        public string Transportista { get; set; }
        public decimal Volumen_Ton { get; set; }
        public DateTime Fecha_Recepcion { get; set; }
        public string Estatus { get; set; }
    }
}
