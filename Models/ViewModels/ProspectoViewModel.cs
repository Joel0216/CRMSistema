using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRMSistema.Models.ViewModels
{
    public class ProspectoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La Razón Social es obligatoria.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Razón Social / Nombre completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El RFC es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        [Display(Name = "RFC")]
        public string Rfc { get; set; }

        [Required(ErrorMessage = "El Nombre Comercial es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Nombre Comercial")]
        public string NombreComercial { get; set; }

        [Required(ErrorMessage = "El tipo de persona es obligatorio.")]
        [Display(Name = "Tipo de persona")]
        public string TipoPersona { get; set; }

        [Required(ErrorMessage = "Debe indicar si tiene sucursales.")]
        [Display(Name = "¿Tiene sucursales?")]
        public string TieneSucursales { get; set; }

        [Required(ErrorMessage = "El nombre de contacto es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Nombre de contacto")]
        public string Contacto { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(30, ErrorMessage = "Máximo 30 caracteres.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Display(Name = "Estatus")]
        public string Estatus { get; set; }

        [Display(Name = "Notas")]
        public string Notas { get; set; }

        // Dirección
        [Required(ErrorMessage = "La calle es obligatoria.")]
        [Display(Name = "Calle")]
        public string Calle { get; set; }

        [Required(ErrorMessage = "El número exterior es obligatorio.")]
        [Display(Name = "Número Exterior")]
        public string NumExt { get; set; }

        [Display(Name = "Número Interior")]
        public string NumInt { get; set; }

        [Required(ErrorMessage = "La colonia es obligatoria.")]
        [Display(Name = "Colonia")]
        public string Colonia { get; set; }

        [Required(ErrorMessage = "El municipio es obligatorio.")]
        [Display(Name = "Municipio")]
        public string Municipio { get; set; }

        [Required(ErrorMessage = "El código postal es obligatorio.")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "El CP debe tener 5 dígitos.")]
        [Display(Name = "Código Postal")]
        public string Cp { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [Display(Name = "Estado")]
        public string Estado { get; set; }

        [Display(Name = "Latitud")]
        public string Lat { get; set; }

        [Display(Name = "Longitud")]
        public string Lng { get; set; }

        [Display(Name = "Coordenadas manuales")]
        public bool CoordenadasManuales { get; set; }

        [Display(Name = "Concesionaria")]
        public string Concesionaria { get; set; }

        [Display(Name = "Referencias")]
        public string Referencias { get; set; }

        [Display(Name = "Folio Catastral")]
        public string FolioCatastral { get; set; }

        [Display(Name = "Días disponibles")]
        public string DiasDisponibles { get; set; }

        [Display(Name = "Horario")]
        public string Horario { get; set; }

        [Display(Name = "Ruta")]
        public string Ruta { get; set; }

        [Display(Name = "Motivo de rechazo")]
        public string MotivoRechazo { get; set; }

        [Display(Name = "Estatus cotización")]
        public string EstatusCotizacion { get; set; }

        [Display(Name = "Motivo rechazo cotización")]
        public string MotivoRechazoCotizacion { get; set; }

        [Display(Name = "Vendedor")]
        public int? VendedorId { get; set; }
        public string VendedorNombre { get; set; }

        public DateTime? Fecha { get; set; }
        public DateTime? FechaRechazo { get; set; }

        // Fotos y documentos (Base64)
        [Required(ErrorMessage = "La foto de fachada es obligatoria.")]
        public string FotoFachada { get; set; }

        [Required(ErrorMessage = "La foto de acceso es obligatoria.")]
        public string FotoAcceso { get; set; }

        [Required(ErrorMessage = "La foto de referencia es obligatoria.")]
        public string FotoReferencia { get; set; }

        public string DocumentoCatastral { get; set; }

        public string DocumentoCatastralNombre { get; set; }

        // Listas hijas
        public List<ProspectoContactoViewModel> Contactos { get; set; } = new List<ProspectoContactoViewModel>();
        public List<ProspectoSucursalViewModel> Sucursales { get; set; } = new List<ProspectoSucursalViewModel>();
    }

    public class ProspectoContactoViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El nombre del contacto es obligatorio.")]
        public string NombreContacto { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato inválido.")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; }

        public bool RepresentanteLegal { get; set; }
    }

    public class ProspectoSucursalViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El nombre de la sucursal es obligatorio.")]
        public string NombreSucursal { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string TelefonoSucursal { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato inválido.")]
        public string CorreoElectronico { get; set; }

        [Required(ErrorMessage = "El responsable es obligatorio.")]
        public string NombreResponsable { get; set; }

        public string Calle { get; set; }
        public string NumExt { get; set; }
        public string NumInt { get; set; }
        public string Colonia { get; set; }
        public string Municipio { get; set; }
        public string Cp { get; set; }
        public string Estado { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
        public string Concesionaria { get; set; }
        public string Referencias { get; set; }
        public string FolioCatastral { get; set; }

        public string FotoFachada { get; set; }
        public string FotoAcceso { get; set; }
        public string FotoReferencia { get; set; }
        public string DocumentoCatastral { get; set; }
        public string DocumentoCatastralNombre { get; set; }
    }
}
