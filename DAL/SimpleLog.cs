using System;
using System.IO;
using System.Web;

namespace CRMSistema.DAL
{
    /// <summary>
    /// Logger mínimo para diagnóstico de errores de base de datos.
    /// Escribe en App_Data/Logs/crm_YYYYMMDD.log; si no puede escribir, ignora el error.
    /// </summary>
    public static class SimpleLog
    {
        private static readonly object Lock = new object();

        public static void Write(string message)
        {
            try
            {
                string baseDir = GetLogDirectory();
                if (!Directory.Exists(baseDir))
                    Directory.CreateDirectory(baseDir);

                string path = Path.Combine(baseDir, $"crm_{DateTime.Now:yyyyMMdd}.log");
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                lock (Lock)
                    File.AppendAllText(path, line);
            }
            catch
            {
                // Nunca debe fallar la operación principal por un problema de log.
            }
        }

        private static string GetLogDirectory()
        {
            try
            {
                if (HttpContext.Current?.Server != null)
                    return HttpContext.Current.Server.MapPath("~/App_Data/Logs");
            }
            catch
            {
                // Si no hay contexto web, usar la carpeta de la aplicación.
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs");
        }
    }
}
