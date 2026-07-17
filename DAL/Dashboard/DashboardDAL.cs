using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CRMSistema.DAL.Dashboard
{
    public class DashboardDAL
    {
        public dynamic ObtenerResumen()
        {
            return AdoHelper.QuerySingle("sp_ObtenerResumenPanel", CommandType.StoredProcedure);
        }

        public List<dynamic> ObtenerTendenciaProspectos()
        {
            return AdoHelper.Query("SP_Dashboard_GetTendenciaProspectos", CommandType.StoredProcedure);
        }

        public List<dynamic> ObtenerTendenciaVentas()
        {
            return AdoHelper.Query("SP_Dashboard_GetTendenciaVentas", CommandType.StoredProcedure);
        }

        public List<dynamic> ObtenerOrigenes()
        {
            return AdoHelper.Query("SP_Dashboard_GetOrigenes", CommandType.StoredProcedure);
        }

        public List<dynamic> ObtenerTiposInmueble()
        {
            return AdoHelper.Query("SP_Dashboard_GetTiposInmueble", CommandType.StoredProcedure);
        }

        public List<dynamic> ObtenerEstatusDistribucion()
        {
            return AdoHelper.Query("SP_Dashboard_GetEstatusDistribucion", CommandType.StoredProcedure);
        }

        public List<dynamic> ObtenerPipeline()
        {
            return AdoHelper.Query("SP_Dashboard_GetPipeline", CommandType.StoredProcedure);
        }

        public List<dynamic> ObtenerCotizacionesDetalle()
        {
            return AdoHelper.Query("SP_Dashboard_GetCotizacionesDetalle", CommandType.StoredProcedure);
        }

        public dynamic ObtenerKPIs(DateTime inicioMes, DateTime finMes, DateTime inicioMesAnt, DateTime finMesAnt)
        {
            return AdoHelper.QuerySingle("SP_Dashboard_GetKPIs", CommandType.StoredProcedure,
                new SqlParameter("@inicioMes", inicioMes),
                new SqlParameter("@finMes", finMes),
                new SqlParameter("@inicioMesAnt", inicioMesAnt),
                new SqlParameter("@finMesAnt", finMesAnt));
        }
    }
}
