namespace CRMSistema.Models.Usuarios
{
    /// <summary>
    /// Request simple usado por acciones que solo necesitan el ID de un usuario
    /// (Anular, Activar) cuando el frontend envía JSON.
    /// </summary>
    public class UsuarioIdRequest
    {
        public int id { get; set; }
    }
}
