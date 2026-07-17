namespace CRMSistema.Models.Common
{
    /// <summary>
    /// Respuesta estándar usada por los controladores MVC y API.
    /// </summary>
    public class ApiResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
}
