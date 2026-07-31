using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CRMSistema.DAL.Dashboard
{
    /// <summary>
    /// Acceso a datos del dashboard. Toda la lógica vive en stored procedures;
    /// esta capa solo orquesta llamadas parametrizadas.
    /// </summary>
    public class DashboardDAL
    {
        public dynamic ObtenerResumen()
        {
            return AdoHelper.QuerySingle("sp_ObtenerResumenPanel", CommandType.StoredProcedure);
        }

        public List<dynamic> ObtenerTendenciaProspectos(int? vendedorId = null)
        {
            return AdoHelper.Query("SP_Dashboard_GetTendenciaProspectos", CommandType.StoredProcedure,
                new SqlParameter("@VendedorId", (object)vendedorId ?? DBNull.Value));
        }

        public List<dynamic> ObtenerTendenciaVentas(int? vendedorId = null)
        {
            return AdoHelper.Query("SP_Dashboard_GetTendenciaVentas", CommandType.StoredProcedure,
                new SqlParameter("@VendedorId", (object)vendedorId ?? DBNull.Value));
        }

        public List<dynamic> ObtenerOrigenes(int? vendedorId = null)
        {
            return AdoHelper.Query("SP_Dashboard_GetOrigenes", CommandType.StoredProcedure,
                new SqlParameter("@VendedorId", (object)vendedorId ?? DBNull.Value));
        }

        public List<dynamic> ObtenerTiposInmueble(int? vendedorId = null)
        {
            return AdoHelper.Query("SP_Dashboard_GetTiposInmueble", CommandType.StoredProcedure,
                new SqlParameter("@VendedorId", (object)vendedorId ?? DBNull.Value));
        }

        public List<dynamic> ObtenerEstatusDistribucionPorMes(DateTime inicioMes, DateTime finMes, int? vendedorId = null)
        {
            return AdoHelper.Query("SP_Dashboard_GetEstatusDistribucionPorMes", CommandType.StoredProcedure,
                new SqlParameter("@inicioMes", inicioMes),
                new SqlParameter("@finMes", finMes),
                new SqlParameter("@VendedorId", (object)vendedorId ?? DBNull.Value));
        }

        public List<dynamic> ObtenerPipeline(int? vendedorId = null)
        {
            return AdoHelper.Query("SP_Dashboard_GetPipeline", CommandType.StoredProcedure,
                new SqlParameter("@VendedorId", (object)vendedorId ?? DBNull.Value));
        }

        public List<dynamic> ObtenerCotizacionesDetalle(int? vendedorId = null)
        {
            return AdoHelper.Query("SP_Dashboard_GetCotizacionesDetalle", CommandType.StoredProcedure,
                new SqlParameter("@VendedorId", (object)vendedorId ?? DBNull.Value));
        }

        public dynamic ObtenerKPIs(DateTime inicioMes, DateTime finMes, DateTime inicioMesAnt, DateTime finMesAnt, int? vendedorId = null)
        {
            return AdoHelper.QuerySingle("SP_Dashboard_GetKPIs", CommandType.StoredProcedure,
                new SqlParameter("@inicioMes", inicioMes),
                new SqlParameter("@finMes", finMes),
                new SqlParameter("@inicioMesAnt", inicioMesAnt),
                new SqlParameter("@finMesAnt", finMesAnt),
                new SqlParameter("@VendedorId", (object)vendedorId ?? DBNull.Value));
        }
    }
}
