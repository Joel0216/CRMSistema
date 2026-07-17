namespace CRMSistema.Models.Contratos
{
    /// <summary>
    /// Placeholder para modelos de contratos autorizados.
    /// </summary>
    public class ContratoDto
    {
        public int Contrato_ID { get; set; }
        public string Folio { get; set; }
        public string Cliente { get; set; }
        public decimal Monto { get; set; }
        public string Estatus { get; set; }
    }
}
