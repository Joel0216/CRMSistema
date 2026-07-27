namespace CRMSistema.Models.Usuarios
{
    /// <summary>
    /// Representación plana de un rol para listados JSON.
    /// </summary>
    public class RolDto
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public bool activo { get; set; }
    }
}
