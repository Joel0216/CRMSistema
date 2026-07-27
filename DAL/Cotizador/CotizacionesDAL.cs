using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CRMSistema.Models.Cotizador;

namespace CRMSistema.DAL.Cotizador
{
    /// <summary>
    /// Acceso a datos para el módulo de cotizaciones.
    /// </summary>
    public class CotizacionesDAL
    {
        public List<ServicioResiduoDto> ObtenerServiciosResiduos()
        {
            return AdoHelper.Query("SP_Cotizaciones_GetServiciosResiduos", CommandType.StoredProcedure)
                .Select(r => new ServicioResiduoDto
                {
                    codigo_control = r.codigo_control?.ToString() ?? "",
                    codigo_sana = r.codigo_sana?.ToString() ?? "",
                    tipo = r.tipo?.ToString() ?? "",
                    descripcion = r.descripcion?.ToString() ?? "",
                    precio = r.precio != null ? Convert.ToDecimal(r.precio) : 0m,
                    unidad_medida = r.unidad_medida?.ToString() ?? ""
                }).ToList();
        }

        public List<UnidadRmeDto> ObtenerUnidadesRme()
        {
            return AdoHelper.Query("SP_Configurador_GetUnidades", CommandType.StoredProcedure)
                .Select(r => new UnidadRmeDto
                {
                    Unidad_ID = r.Unidad_ID != null ? Convert.ToInt32(r.Unidad_ID) : 0,
                    Nombre_Unidad = r.Nombre_Unidad?.ToString() ?? "",
                    Capacidad_Toneladas = r.Capacidad_Toneladas != null ? Convert.ToDecimal(r.Capacidad_Toneladas) : 0m,
                    Costo_Unitario = r.Costo_Unitario != null ? Convert.ToDecimal(r.Costo_Unitario) : 0m
                }).ToList();
        }

        public List<BorradorDto> ObtenerBorradores(int prospectoId)
        {
            return AdoHelper.Query("SP_Cotizaciones_GetBorradores", CommandType.StoredProcedure,
                    new SqlParameter("@Prospecto_ID", prospectoId))
                .Select(r => new BorradorDto
                {
                    Borrador_ID = r.Borrador_ID != null ? Convert.ToInt32(r.Borrador_ID) : 0,
                    Prospecto_ID = r.Prospecto_ID != null ? Convert.ToInt32(r.Prospecto_ID) : 0,
                    Datos_Borrador = r.Datos_Borrador?.ToString() ?? "",
                    Fecha_Creacion = r.Fecha_Creacion != null ? ((DateTime)r.Fecha_Creacion).ToString("yyyy-MM-ddTHH:mm:ss") : null
                }).ToList();
        }

        public int CrearBorrador(int prospectoId, string datosJson)
        {
            using (var db = Db.GetConnection())
            using (var cmd = new SqlCommand("SP_Cotizaciones_CreateBorrador", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Prospecto_ID", prospectoId);
                cmd.Parameters.AddWithValue("@Datos_Borrador", datosJson);
                var pId = new SqlParameter("@NuevoBorrador_ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pId);

                db.Open();
                cmd.ExecuteNonQuery();
                return pId.Value != null ? Convert.ToInt32(pId.Value) : 0;
            }
        }

        public void EliminarBorrador(int borradorId)
        {
            AdoHelper.Execute("SP_Cotizaciones_DeleteBorrador", CommandType.StoredProcedure,
                new SqlParameter("@Borrador_ID", borradorId));
        }

        public void ActualizarBorrador(int borradorId, string datosJson)
        {
            AdoHelper.Execute("SP_Cotizaciones_UpdateBorrador", CommandType.StoredProcedure,
                new SqlParameter("@Borrador_ID", borradorId),
                new SqlParameter("@Datos_Borrador", datosJson));
        }

        public void EnviarCotizacion(int prospectoId, string email, string nombre, string passwordTemporal)
        {
            AdoHelper.Execute("SP_Cotizaciones_Enviar", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", prospectoId),
                new SqlParameter("@Email", email),
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@Password_Temporal", passwordTemporal));
        }

        public long CrearServicioCotizado(ServicioCotizadoModel model)
        {
            using (var db = Db.GetConnection())
            using (var cmd = new SqlCommand("SP_ServiciosCotizados_Insert", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Trato_ID", model.trato_id);
                cmd.Parameters.AddWithValue("@Tipo_Residuo", model.tipo_residuo);
                cmd.Parameters.AddWithValue("@Frecuencia", model.frecuencia);
                cmd.Parameters.AddWithValue("@Periodicidad_Pago", model.periodicidad_pago);
                cmd.Parameters.AddWithValue("@Volumen_Estimado", (object)model.volumen_estimado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Precio_Unitario", (object)model.precio_unitario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Dias_Asignados", model.dias_asignados);
                cmd.Parameters.AddWithValue("@Porcentaje_Adicional", (object)model.porcentaje_adicional ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Porcentaje_Descuento", (object)model.porcentaje_descuento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Sucursal_ID", (object)model.sucursal_id ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tipo_Unidad", (object)model.tipo_unidad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tipo_Cobro", (object)model.tipo_cobro ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Recolectores", (object)model.recolectores ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Turno", (object)model.turno ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Ruta", (object)model.ruta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Costo_Tonelada", (object)model.costo_tonelada ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Costo_Disposicion", (object)model.costo_disposicion ?? DBNull.Value);

                db.Open();
                return Convert.ToInt64(cmd.ExecuteScalar() ?? 0);
            }
        }

        public List<ServicioCotizadoModel> ObtenerServiciosCotizados(int tratoId)
        {
            return AdoHelper.Query<ServicioCotizadoModel>("SP_ServiciosCotizados_GetByTrato", CommandType.StoredProcedure,
                new SqlParameter("@Trato_ID", tratoId));
        }

        public void ActualizarServicioCotizado(int id, ServicioCotizadoModel model)
        {
            AdoHelper.Execute("SP_ServiciosCotizados_Update", CommandType.StoredProcedure,
                new SqlParameter("@ID", id),
                new SqlParameter("@Tipo_Residuo", model.tipo_residuo),
                new SqlParameter("@Frecuencia", model.frecuencia ?? "Semanal"),
                new SqlParameter("@Periodicidad_Pago", model.periodicidad_pago ?? "Mensual"),
                new SqlParameter("@Volumen_Estimado", (object)model.volumen_estimado ?? DBNull.Value),
                new SqlParameter("@Precio_Unitario", (object)model.precio_unitario ?? DBNull.Value),
                new SqlParameter("@Dias_Asignados", model.dias_asignados ?? ""),
                new SqlParameter("@Porcentaje_Adicional", (object)model.porcentaje_adicional ?? DBNull.Value),
                new SqlParameter("@Porcentaje_Descuento", (object)model.porcentaje_descuento ?? DBNull.Value),
                new SqlParameter("@Sucursal_ID", (object)model.sucursal_id ?? DBNull.Value),
                new SqlParameter("@Tipo_Unidad", (object)model.tipo_unidad ?? DBNull.Value),
                new SqlParameter("@Tipo_Cobro", (object)model.tipo_cobro ?? DBNull.Value),
                new SqlParameter("@Recolectores", (object)model.recolectores ?? DBNull.Value),
                new SqlParameter("@Turno", (object)model.turno ?? DBNull.Value),
                new SqlParameter("@Ruta", (object)model.ruta ?? DBNull.Value),
                new SqlParameter("@Costo_Tonelada", (object)model.costo_tonelada ?? DBNull.Value),
                new SqlParameter("@Costo_Disposicion", (object)model.costo_disposicion ?? DBNull.Value));
        }

        public void EliminarServicioCotizado(int id)
        {
            AdoHelper.Execute("SP_ServiciosCotizados_Delete", CommandType.StoredProcedure,
                new SqlParameter("@ID", id));
        }

        // ─────────────────────────────────────────────────────────
        // Validación de cotizaciones
        // ─────────────────────────────────────────────────────────
        public int CrearValidacion(CotizacionValidacionModel model)
        {
            using (var db = Db.GetConnection())
            using (var cmd = new SqlCommand("SP_CotizacionesValidacion_Insert", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Prospecto_ID", model.Prospecto_ID);
                cmd.Parameters.AddWithValue("@Borrador_ID", model.Borrador_ID);
                cmd.Parameters.AddWithValue("@Datos_Cotizacion", model.Datos_Cotizacion);
                cmd.Parameters.AddWithValue("@Usuario_Creacion", (object)model.Usuario_Creacion ?? DBNull.Value);

                db.Open();
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }
        }

        public List<CotizacionValidacionModel> ObtenerValidacionesPendientes()
        {
            return AdoHelper.Query<CotizacionValidacionModel>("SP_CotizacionesValidacion_GetPending", CommandType.StoredProcedure);
        }

        public CotizacionValidacionModel ObtenerValidacionPorId(int id)
        {
            return AdoHelper.QuerySingle<CotizacionValidacionModel>("SP_CotizacionesValidacion_GetById", CommandType.StoredProcedure,
                new SqlParameter("@Validacion_ID", id));
        }

        public CotizacionValidacionModel ObtenerValidacionPorProspecto(int prospectoId)
        {
            return AdoHelper.QuerySingle<CotizacionValidacionModel>("SP_CotizacionesValidacion_GetByProspecto", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", prospectoId));
        }

        public List<CotizacionValidacionModel> ObtenerValidacionesPorProspecto(int prospectoId)
        {
            return AdoHelper.Query<CotizacionValidacionModel>("SP_CotizacionesValidacion_GetByProspecto", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", prospectoId));
        }

        public CotizacionValidacionModel ObtenerValidacionPorBorrador(int borradorId)
        {
            // Preferir la validación autorizada más reciente; si no hay autorizadas,
            // devolver la última cualquiera para mantener compatibilidad con el flujo de revisión.
            return AdoHelper.QuerySingle<CotizacionValidacionModel>("SP_CotizacionesValidacion_GetByBorrador", CommandType.StoredProcedure,
                new SqlParameter("@Borrador_ID", borradorId));
        }

        public void ActualizarEstatusValidacion(int id, string estatus, string motivoRechazo, string usuarioValida)
        {
            AdoHelper.Execute("SP_CotizacionesValidacion_UpdateEstatus", CommandType.StoredProcedure,
                new SqlParameter("@Validacion_ID", id),
                new SqlParameter("@Estatus", estatus),
                new SqlParameter("@Motivo_Rechazo", (object)motivoRechazo ?? DBNull.Value),
                new SqlParameter("@Usuario_Valida", (object)usuarioValida ?? DBNull.Value));
        }
    }
}
