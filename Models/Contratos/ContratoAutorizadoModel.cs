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
        public string Num_Int { get; set; }
        public string Colonia { get; set; }
        public string Municipio { get; set; }
        public string CP { get; set; }
        public string Estado { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Contacto { get; set; }
        public string Nombre_Comercial { get; set; }
        public string Tipo_Persona { get; set; }
        public string Referencias { get; set; }
        public string Folio_Catastral { get; set; }
        public string Dias_Disponibles { get; set; }
        public string Horario { get; set; }
        public string Ruta { get; set; }
        public string VendedorNombre { get; set; }
    }
}
