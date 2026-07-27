namespace CRMSistema.Models.Usuarios
{
    /// <summary>
    /// Request para crear o editar un usuario desde el panel de administración.
    /// </summary>
    public class UsuarioCrudRequest
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string apellidos { get; set; }
        public string correo { get; set; }
        public string usuario { get; set; }
        public string password { get; set; }
        public int rolId { get; set; }
    }
}
