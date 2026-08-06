using System;
using System.Data.SqlClient;

namespace CRMSistema.DAL
{
    /// <summary>
    /// Traduce excepciones de SQL Server a mensajes útiles para el usuario.
    /// </summary>
    public static class SqlErrorHelper
    {
        /// <summary>
        /// Número de error de ADO.NET cuando se agota CommandTimeout.
        /// </summary>
        public const int TimeoutErrorNumber = -2;

        public static string BuildMessage(Exception ex, string contexto = null)
        {
            if (ex is SqlException sqlEx)
            {
                if (sqlEx.Number == TimeoutErrorNumber)
                {
                    return $"Se agotó el tiempo de espera de la base de datos al {(contexto ?? "guardar")}. " +
                           "Esto suele pasar porque otra ventana del CRM o del servidor tiene una transacción abierta que bloquea la tabla, " +
                           "o porque el stored procedure está tardando más de lo normal. " +
                           "Cierra otras pestañas/ventanas del CRM, espera unos segundos e intenta de nuevo. " +
                           "Si persiste, revisa el log de la aplicación (App_Data/Logs) o ejecuta sp_who2 en SQL Server para ver bloqueos.";
                }
                return $"Error de base de datos ({sqlEx.Number}): {sqlEx.Message}";
            }

            return $"Error al {(contexto ?? "guardar")}: {ex.Message}";
        }
    }
}
