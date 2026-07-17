using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

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

        public List<dynamic> ObtenerEstatusDistribucionPorMes(DateTime inicioMes, DateTime finMes)
        {
            try
            {
                return AdoHelper.Query("SP_Dashboard_GetEstatusDistribucionPorMes", CommandType.StoredProcedure,
                    new SqlParameter("@inicioMes", inicioMes),
                    new SqlParameter("@finMes", finMes));
            }
            catch
            {
                // Fallback: calcular desde crm_prospectos usando Fecha_Creacion si existe, sino devolver estatus actual
                return CalcularEstatusDistribucionPorMesDesdeProspectos(inicioMes, finMes);
            }
        }

        public List<dynamic> CalcularEstatusDistribucionPorMesDesdeProspectos(DateTime inicioMes, DateTime finMes)
        {
            try
            {
                // Si la tabla tiene Fecha_Creacion, filtrar por mes
                var sql = @"SELECT Estatus AS estatus, COUNT(*) AS cantidad
                            FROM crm_prospectos
                            WHERE Fecha_Creacion BETWEEN @inicio AND @fin
                            GROUP BY Estatus";
                var rows = AdoHelper.Query(sql, CommandType.Text,
                    new SqlParameter("@inicio", inicioMes),
                    new SqlParameter("@fin", finMes)).ToList();
                if (rows.Count > 0) return rows;
            }
            catch { }

            // Si no hay Fecha_Creacion o no hay registros filtrados para ese mes,
            // no podemos devolver datos historicos reales; regresamos vacio.
            // El controller completara con la distribucion actual solo si se solicita el mes actual.
            return new List<dynamic>();
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

        // ============================================================
        // FALLBACKS: calculan KPIs desde tablas cuando los SPs del dashboard no devuelven datos
        // ============================================================

        public double IngresosMes(DateTime inicioMes, DateTime finMes)
        {
            try
            {
                var sql = @"SELECT COALESCE(SUM(Importe),0) AS total
                             FROM crm_tratos
                             WHERE Fecha_Creacion BETWEEN @inicio AND @fin";
                var r = AdoHelper.QuerySingle(sql, CommandType.Text,
                    new SqlParameter("@inicio", inicioMes),
                    new SqlParameter("@fin", finMes));
                return Convert.ToDouble(r?.total ?? 0);
            }
            catch
            {
                try
                {
                    var r = AdoHelper.QuerySingle(
                        "SELECT COALESCE(SUM(Importe),0) AS total FROM crm_tratos WHERE Fecha_Cierre_Estimada BETWEEN @inicio AND @fin",
                        CommandType.Text,
                        new SqlParameter("@inicio", inicioMes),
                        new SqlParameter("@fin", finMes));
                    return Convert.ToDouble(r?.total ?? 0);
                }
                catch { return 0; }
            }
        }

        public int ContarProspectos(DateTime inicioMes, DateTime finMes)
        {
            try
            {
                var r = AdoHelper.QuerySingle(
                    "SELECT COUNT(*) AS total FROM crm_prospectos WHERE Fecha_Creacion BETWEEN @inicio AND @fin",
                    CommandType.Text,
                    new SqlParameter("@inicio", inicioMes),
                    new SqlParameter("@fin", finMes));
                return Convert.ToInt32(r?.total ?? 0);
            }
            catch
            {
                try
                {
                    var r = AdoHelper.QuerySingle("SELECT COUNT(*) AS total FROM crm_prospectos", CommandType.Text);
                    return Convert.ToInt32(r?.total ?? 0);
                }
                catch { return 0; }
            }
        }

        public int ContarCotizacionesActivas()
        {
            try
            {
                int tratos = 0, borradores = 0;
                try
                {
                    var r = AdoHelper.QuerySingle("SELECT COUNT(*) AS total FROM crm_tratos WHERE Fase_ID IN (1,2,3)", CommandType.Text);
                    tratos = Convert.ToInt32(r?.total ?? 0);
                }
                catch { }
                try
                {
                    var r = AdoHelper.QuerySingle("SELECT COUNT(*) AS total FROM crm_cotizaciones_borradores", CommandType.Text);
                    borradores = Convert.ToInt32(r?.total ?? 0);
                }
                catch { }
                return tratos + borradores;
            }
            catch { return 0; }
        }

        public int ContarDeudores()
        {
            try
            {
                var r = AdoHelper.QuerySingle(
                    "SELECT COUNT(*) AS total FROM crm_prospectos WHERE LOWER(Estatus) LIKE '%adeudo%'",
                    CommandType.Text);
                return Convert.ToInt32(r?.total ?? 0);
            }
            catch { return 0; }
        }

        public int ContarAlCorriente()
        {
            try
            {
                var r = AdoHelper.QuerySingle(
                    "SELECT COUNT(*) AS total FROM crm_prospectos WHERE LOWER(Estatus) IN ('aprobado','cotizado','en seguimiento')",
                    CommandType.Text);
                return Convert.ToInt32(r?.total ?? 0);
            }
            catch { return 0; }
        }

        public int ContarProspectosConSucursales()
        {
            try
            {
                var r = AdoHelper.QuerySingle(
                    "SELECT COUNT(DISTINCT Prospecto_ID) AS total FROM crm_prospecto_sucursales",
                    CommandType.Text);
                return Convert.ToInt32(r?.total ?? 0);
            }
            catch { return 0; }
        }

        public List<dynamic> PipelineDesdeProspectos()
        {
            try
            {
                var sql = @"SELECT
                                p.Prospecto_ID AS id,
                                ISNULL(p.Nombre_Comercial_Empresa, p.Nombre_Prospecto) AS empresa,
                                p.Nombre_Prospecto AS contacto_principal,
                                p.Estatus AS estatus,
                                p.Tiene_Sucursales AS tieneSucursales,
                                ISNULL(t.Importe, 0) AS monto,
                                p.Fecha_Creacion AS fecha
                            FROM crm_prospectos p
                            LEFT JOIN (
                                SELECT Prospecto_ID, MAX(Trato_ID) AS ultimo
                                FROM crm_tratos
                                GROUP BY Prospecto_ID
                            ) tmax ON tmax.Prospecto_ID = p.Prospecto_ID
                            LEFT JOIN crm_tratos t ON t.Trato_ID = tmax.ultimo
                            ORDER BY p.Fecha_Creacion DESC";
                var rows = AdoHelper.Query(sql, CommandType.Text).ToList();

                // Complementar con contactos y sucursales por prospecto
                try
                {
                    var ids = rows.Select(r => (int)Convert.ToInt32(r.id)).Distinct().ToList();
                    if (ids.Count > 0)
                    {
                        var contactosSql = @"SELECT Prospecto_ID, Nombre_Contacto, Correo, Telefono
                                             FROM crm_prospecto_contactos
                                             WHERE Prospecto_ID IN (" + string.Join(",", ids) + @")";
                        var contactosRows = AdoHelper.Query(contactosSql, CommandType.Text).ToList();

                        var sucursalesSql = @"SELECT Prospecto_ID, Nombre_Sucursal, Nombre_Responsable
                                              FROM crm_prospecto_sucursales
                                              WHERE Prospecto_ID IN (" + string.Join(",", ids) + @")";
                        var sucursalesRows = AdoHelper.Query(sucursalesSql, CommandType.Text).ToList();

                        foreach (var row in rows)
                        {
                            var pid = (int)Convert.ToInt32(row.id);
                            var contactos = contactosRows
                                .Where(c => Convert.ToInt32(c.Prospecto_ID) == pid)
                                .Select(c => Convert.ToString(c.Nombre_Contacto))
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToList();
                            var sucursales = sucursalesRows
                                .Where(s => Convert.ToInt32(s.Prospecto_ID) == pid)
                                .Select(s => Convert.ToString(s.Nombre_Sucursal))
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToList();

                            row.contactos = string.Join(", ", contactos);
                            row.sucursales = string.Join(", ", sucursales);
                            row.tieneSucursales = sucursales.Count > 0 ? "Sí" : "No";
                        }
                    }
                }
                catch { }

                return rows;
            }
            catch
            {
                return new List<dynamic>();
            }
        }
    }
}
