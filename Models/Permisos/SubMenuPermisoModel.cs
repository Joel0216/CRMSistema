namespace CRMSistema.Models.Permisos
{
    /// <summary>
    /// Representa un submenú / enlace dentro de un menú principal.
    /// </summary>
    public class SubMenuPermisoModel
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public string Nombre { get; set; }
        public string Controlador { get; set; }
        public string Accion { get; set; }
        public string Icono { get; set; }
        public int Orden { get; set; }
    }
}
