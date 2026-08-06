using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CRMSistema.Models.Manifiestos;

namespace CRMSistema.DAL.Manifiestos
{
    /// <summary>
    /// Acceso a datos para el módulo de manifiestos.
    /// Usa los stored procedures sugeridos para crm_manifiestos.
    /// Si los SP o la tabla aún no existen, los métodos lanzan excepción
    /// para que el controller pueda caer en el modo demostración.
    /// </summary>
    public class ManifiestosDAL
    {
        public List<dynamic> ObtenerTodos()
        {
            return AdoHelper.Query("SP_Manifiestos_GetAll", CommandType.StoredProcedure);
        }

        public dynamic ObtenerPorId(int id)
        {
            var rows = AdoHelper.Query("SP_Manifiestos_GetById", CommandType.StoredProcedure,
                new SqlParameter("@Manifiesto_ID", id));
            return rows.FirstOrDefault();
        }

        public int Guardar(ManifiestoFormModel m, int? creadoPor = null)
        {
            if (m.Id.HasValue && m.Id.Value > 0)
            {
                AdoHelper.Execute("SP_Manifiestos_Update", CommandType.StoredProcedure,
                    new SqlParameter("@Manifiesto_ID", m.Id.Value),
                    new SqlParameter("@Folio", m.Folio ?? ""),
                    new SqlParameter("@Fecha_Recepcion", (object)m.Fecha ?? DBNull.Value),
                    new SqlParameter("@Generador", m.Generador ?? ""),
                    new SqlParameter("@Transportista", m.Transportista ?? ""),
                    new SqlParameter("@DestinoFinal", m.Destino ?? ""),
                    new SqlParameter("@Volumen_Ton", (object)m.Volumen ?? DBNull.Value),
                    new SqlParameter("@Estatus", m.Estatus ?? ""));
                return m.Id.Value;
            }

            var pId = new SqlParameter("@Manifiesto_ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            AdoHelper.Execute("SP_Manifiestos_Insert", CommandType.StoredProcedure,
                new SqlParameter("@Folio", m.Folio ?? ""),
                new SqlParameter("@Fecha_Recepcion", (object)m.Fecha ?? DBNull.Value),
                new SqlParameter("@Generador", m.Generador ?? ""),
                new SqlParameter("@Transportista", m.Transportista ?? ""),
                new SqlParameter("@DestinoFinal", m.Destino ?? ""),
                new SqlParameter("@Volumen_Ton", (object)m.Volumen ?? DBNull.Value),
                new SqlParameter("@Estatus", m.Estatus ?? ""),
                new SqlParameter("@Creado_Por", (object)creadoPor ?? DBNull.Value),
                pId);
            return pId.Value != null ? Convert.ToInt32(pId.Value) : 0;
        }

        public void Eliminar(int id)
        {
            AdoHelper.Execute("SP_Manifiestos_Delete", CommandType.StoredProcedure,
                new SqlParameter("@Manifiesto_ID", id));
        }

        public void CambiarEstatus(int id, string estatus)
        {
            AdoHelper.Execute("SP_Manifiestos_UpdateEstatus", CommandType.StoredProcedure,
                new SqlParameter("@Manifiesto_ID", id),
                new SqlParameter("@Estatus", estatus ?? ""));
        }
    }
}
