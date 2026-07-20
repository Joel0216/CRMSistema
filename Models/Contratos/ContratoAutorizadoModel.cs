using System;

namespace CRMSistema.Models.Contratos
{
    /// <summary>
    /// Contrato autorizado a partir de una cotización aprobada.
    /// </summary>
    public class ContratoAutorizadoModel
    {
        public int Contrato_ID { get; set; }
        public int Prospecto_ID { get; set; }
        public int Validacion_ID { get; set; }
        public string Folio { get; set; }
        public decimal? Monto_Mensual { get; set; }
        public string Estatus { get; set; }
        public DateTime Fecha_Autorizacion { get; set; }
        public string Autorizado_Por { get; set; }

        // Campos del prospecto
        public string RazonSocial { get; set; }
        public string RFC { get; set; }
        public string Calle { get; set; }
        public string Num_Ext { get; set; }
        public string Colonia { get; set; }
        public string Municipio { get; set; }
    }
}
