using System.Collections.Generic;

namespace CRMSistema.Models.Permisos
{
    /// <summary>
    /// Representa un menú principal con la lista de submenús permitidos para un rol.
    /// </summary>
    public class MenuPermisoModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Icono { get; set; }
        public int Orden { get; set; }
        public List<SubMenuPermisoModel> SubMenus { get; set; } = new List<SubMenuPermisoModel>();
    }
}
