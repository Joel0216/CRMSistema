using System.Collections.Generic;
using CRMSistema.Models.Cotizador;

namespace CRMSistema.Models.Contratos
{
    /// <summary>
    /// Payload para guardar la edición de servicios cotizados de un contrato rechazado.
    /// </summary>
    public class GuardarServiciosRequest
    {
        public int contrato_id { get; set; }
        public List<ServicioCotizadoModel> servicios { get; set; }

        /// <summary>
        /// Datos del cliente editables desde la pantalla de contratos rechazados.
        /// </summary>
        public ClienteContratoEdicion cliente { get; set; }
    }

    public class ClienteContratoEdicion
    {
        public string razon_social { get; set; }
        public string rfc { get; set; }
        public string nombre_comercial { get; set; }
        public string representante_legal { get; set; }
        public string domicilio_fiscal { get; set; }
        public string domicilio_recoleccion { get; set; }
        public string telefono { get; set; }
        public string correo { get; set; }
        public string folio_catastral { get; set; }
    }

    /// <summary>
    /// Payload para guardar dirección, folio y archivos del prospecto desde contratos.
    /// </summary>
    public class GuardarDireccionProspectoRequest
    {
        public int contrato_id { get; set; }
        public string fiscal_calle { get; set; }
        public string fiscal_num_ext { get; set; }
        public string fiscal_num_int { get; set; }
        public string fiscal_colonia { get; set; }
        public string fiscal_municipio { get; set; }
        public string fiscal_cp { get; set; }
        public string fiscal_estado { get; set; }

        public string recoleccion_calle { get; set; }
        public string recoleccion_num_ext { get; set; }
        public string recoleccion_num_int { get; set; }
        public string recoleccion_colonia { get; set; }
        public string recoleccion_municipio { get; set; }
        public string recoleccion_cp { get; set; }
        public string recoleccion_estado { get; set; }

        public string folio_catastral { get; set; }
    }
}
