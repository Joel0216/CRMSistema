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

        public List<ContratoAutorizadoModel> ObtenerContratosPorEstatus(string estatus)
        {
            return AdoHelper.Query<ContratoAutorizadoModel>("SP_ContratosAutorizados_GetByEstatus", CommandType.StoredProcedure,
                new SqlParameter("@Estatus", (object)estatus ?? DBNull.Value));
        }

        public List<ContratoAutorizadoModel> ObtenerContratosPorAutorizar()
        {
            return AdoHelper.Query<ContratoAutorizadoModel>("SP_ContratosAutorizados_GetPending", CommandType.StoredProcedure);
        }

        public ContratoAutorizadoModel ObtenerPorId(int contratoId)
        {
            return AdoHelper.QuerySingle<ContratoAutorizadoModel>("SP_ContratosAutorizados_GetById", CommandType.StoredProcedure,
                new SqlParameter("@Contrato_ID", contratoId));
        }

        public ContratoAutorizadoModel ObtenerPorValidacionId(int validacionId)
        {
            return AdoHelper.QuerySingle<ContratoAutorizadoModel>("SP_ContratosAutorizados_GetByValidacion", CommandType.StoredProcedure,
                new SqlParameter("@Validacion_ID", validacionId));
        }

        public void ActualizarEstatus(int contratoId, string estatus, string motivoRechazo = null, string usuarioRechaza = null)
        {
            AdoHelper.Execute("SP_ContratosAutorizados_UpdateEstatus", CommandType.StoredProcedure,
                new SqlParameter("@Contrato_ID", contratoId),
                new SqlParameter("@Estatus", estatus ?? ""),
                new SqlParameter("@Motivo_Rechazo", (object)motivoRechazo ?? DBNull.Value),
                new SqlParameter("@Usuario_Rechaza", (object)usuarioRechaza ?? DBNull.Value));
        }

        public void ActualizarMontoMensual(int contratoId, decimal monto)
        {
            AdoHelper.Execute("SP_ContratosAutorizados_UpdateMontoMensual", CommandType.StoredProcedure,
                new SqlParameter("@Contrato_ID", contratoId),
                new SqlParameter("@Monto_Mensual", monto));
        }
    }
}
