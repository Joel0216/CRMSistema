using System.Collections.Generic;

namespace CRMSistema.Models.Prospectos
{
    public class ApiProspectoModel
    {
        public string id { get; set; }
        public string nombre { get; set; }
        public string rfc { get; set; }
        public string contacto { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public string tipoPersona { get; set; }
        public string tieneSucursales { get; set; }
        public string estatus { get; set; }
        public string tipoInmueble { get; set; }
        public string notas { get; set; }
        public string calle { get; set; }
        public string numExt { get; set; }
        public string numInt { get; set; }
        public string colonia { get; set; }
        public string municipio { get; set; }
        public string cp { get; set; }
        public string estado { get; set; }
        public string dias_disponibles { get; set; }
        public string horario { get; set; }
        public string ruta { get; set; }
        public string nombreComercial { get; set; }
        public string concesionaria { get; set; }
        public string referencias { get; set; }
        public string folioCatastral { get; set; }
        public string foto_comprobante { get; set; }
        public string foto_fachada { get; set; }
        public string foto_acceso { get; set; }
        public string foto_referencia { get; set; }
        public string documento_catastral { get; set; }
        public string documento_catastral_nombre { get; set; }
        public decimal? lat { get; set; }
        public decimal? lng { get; set; }
        public bool coordenadas_manuales { get; set; }

        public List<ApiSucursalModel> sucursales { get; set; }
        public List<ApiContactoModel> contactos { get; set; }
    }

    public class ApiSucursalModel
    {
        public string nombre_sucursal { get; set; }
        public string correo_electronico { get; set; }
        public string telefono_sucursal { get; set; }
        public string nombre_responsable { get; set; }
        public string calle { get; set; }
        public string numExt { get; set; }
        public string numInt { get; set; }
        public string colonia { get; set; }
        public string municipio { get; set; }
        public string cp { get; set; }
        public string estado { get; set; }
        public string concesionaria { get; set; }
        public string referencias { get; set; }
        public string folioCatastral { get; set; }
        public decimal? lat { get; set; }
        public decimal? lng { get; set; }
        public string foto_comprobante { get; set; }
        public string foto_fachada { get; set; }
        public string foto_acceso { get; set; }
        public string foto_referencia { get; set; }
        public string documento_catastral { get; set; }
        public string documento_catastral_nombre { get; set; }
    }

    public class ApiContactoModel
    {
        public string nombre_contacto { get; set; }
        public string correo { get; set; }
        public bool representante_legal { get; set; }
        public string telefono { get; set; }
    }

    public class ApiNotificacionModel
    {
        public string tipo_asunto { get; set; }
        public string correo_destino { get; set; }
        public int? enviado_por { get; set; }
        public string password_temporal { get; set; }
        public string cotizacion_ref { get; set; }
        public string vigencia_inicio { get; set; }
        public string vigencia_fin { get; set; }
    }

    public class ApiArchivoModel
    {
        public string base64 { get; set; }
        public string nombre { get; set; }
        public string tipo { get; set; }
        public int peso { get; set; }
    }
}
