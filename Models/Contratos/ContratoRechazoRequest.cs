namespace CRMSistema.Models.Contratos
{
    /// <summary>
    /// Payload para rechazar un contrato por parte del supervisor.
    /// </summary>
    public class ContratoRechazoRequest
    {
        public int contrato_id { get; set; }
        public string motivo { get; set; }
    }
}
