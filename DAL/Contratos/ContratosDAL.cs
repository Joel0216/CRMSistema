using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CRMSistema.Models.Contratos;

namespace CRMSistema.DAL.Contratos
{
    /// <summary>
    /// Acceso a datos para el módulo de contratos autorizados.
    /// </summary>
    public class ContratosDAL
    {
        public int CrearContratoAutorizado(ContratoAutorizadoModel model)
        {
            using (var db = Db.GetConnection())
            using (var cmd = new SqlCommand("SP_ContratosAutorizados_Insert", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Prospecto_ID", model.Prospecto_ID);
                cmd.Parameters.AddWithValue("@Validacion_ID", model.Validacion_ID);
                cmd.Parameters.AddWithValue("@Folio", model.Folio);
                cmd.Parameters.AddWithValue("@Monto_Mensual", (object)model.Monto_Mensual ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Autorizado_Por", (object)model.Autorizado_Por ?? DBNull.Value);

                db.Open();
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }
        }

        public List<ContratoAutorizadoModel> ObtenerContratosAutorizados()
        {
            return AdoHelper.Query<ContratoAutorizadoModel>("SP_ContratosAutorizados_GetAll", CommandType.StoredProcedure);
        }
    }
}
