using System;

namespace CRMSistema.Models.Cotizador
{
    /// <summary>
    /// Solicitud de validación de una cotización.
    /// </summary>
    public class CotizacionValidacionModel
    {
        public int Validacion_ID { get; set; }
        public int Prospecto_ID { get; set; }
        public int Borrador_ID { get; set; }
        public string Datos_Cotizacion { get; set; }
        public string Estatus { get; set; }
        public string Motivo_Rechazo { get; set; }
        public DateTime Fecha_Creacion { get; set; }
        public DateTime? Fecha_Actualizacion { get; set; }
        public string Usuario_Creacion { get; set; }
        public string Usuario_Valida { get; set; }

        // Campos del prospecto (join)
        public string RazonSocial { get; set; }
        public string RFC { get; set; }
        public string Nombre_Comercial { get; set; }
        public string Calle { get; set; }
        public string Num_Ext { get; set; }
        public string Colonia { get; set; }
        public string Municipio { get; set; }
        public string CP { get; set; }
        public string Estado { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Tipo_Persona { get; set; }
        public string VendedorNombre { get; set; }
    }
}
