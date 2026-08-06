using System;

namespace CRMSistema.Models.Manifiestos
{
    /// <summary>
    /// Modelo ligero para capturar un nuevo manifiesto desde la vista.
    /// NOTA: Campos de demostración; ajustar al esquema real de crm_manifiestos.
    /// </summary>
    public class ManifiestoFormModel
    {
        public int? Id { get; set; }
        public string Folio { get; set; }
        public DateTime? Fecha { get; set; }
        public string Generador { get; set; }
        public string Transportista { get; set; }
        public string Destino { get; set; }
        public decimal? Volumen { get; set; }
        public string Estatus { get; set; }
    }
}
