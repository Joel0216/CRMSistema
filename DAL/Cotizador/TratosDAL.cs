using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CRMSistema.Models.Cotizador;

namespace CRMSistema.DAL.Cotizador
{
    /// <summary>
    /// Acceso a datos para el módulo de tratos/oportunidades.
    /// </summary>
    public class TratosDAL
    {
        public int Crear(TratoModel model)
        {
            using (var db = Db.GetConnection())
            using (var cmd = new SqlCommand("SP_Tratos_Insert", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Prospecto_ID", model.prospecto_id);
                cmd.Parameters.AddWithValue("@Nombre_Trato", model.nombre_trato);
                cmd.Parameters.AddWithValue("@Importe", model.importe);
                cmd.Parameters.AddWithValue("@Fase_ID", model.fase_id);
                cmd.Parameters.AddWithValue("@Fecha_Cierre_Estimada",
                    string.IsNullOrEmpty(model.fecha_limite_cotizacion) ? (object)DBNull.Value : (object)model.fecha_limite_cotizacion);

                var pId = new SqlParameter("@NuevoTrato_ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pId);

                db.Open();
                cmd.ExecuteNonQuery();
                return pId.Value != null && pId.Value != DBNull.Value ? Convert.ToInt32(pId.Value) : 0;
            }
        }

        public List<TratoModel> ObtenerPorProspecto(int prospectoId)
        {
            return AdoHelper.Query<TratoModel>("SP_Tratos_GetByProspecto", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", prospectoId));
        }
    }
}
