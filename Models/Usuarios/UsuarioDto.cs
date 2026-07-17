namespace CRMSistema.Models.Usuarios
{
    /// <summary>
    /// Representación plana de un usuario para las respuestas JSON de la API.
    /// </summary>
    public class UsuarioDto
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string correo { get; set; }
        public string rol { get; set; }
    }
}
