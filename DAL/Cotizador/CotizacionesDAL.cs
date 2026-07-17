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
                cmd.Parameters.AddWithValue("@Sucursal_ID", model.sucursal_id);
                cmd.Parameters.AddWithValue("@Tipo_Unidad", model.tipo_unidad);
                cmd.Parameters.AddWithValue("@Tipo_Cobro", model.tipo_cobro);
                cmd.Parameters.AddWithValue("@Recolectores", (object)model.recolectores ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Turno", model.turno);
                cmd.Parameters.AddWithValue("@Ruta", model.ruta);
                cmd.Parameters.AddWithValue("@Recorrido_Servicio", (object)model.recorrido_km ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Costo_Tonelada", (object)model.costo_tonelada ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Costo_Disposicion", (object)model.costo_disposicion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Costo_Renta", (object)model.costo_renta_base ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Capacidad_Toneladas", (object)model.capacidad_toneladas ?? DBNull.Value);

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
                new SqlParameter("@Frecuencia", model.frecuencia),
                new SqlParameter("@Periodicidad_Pago", model.periodicidad_pago),
                new SqlParameter("@Volumen_Estimado", (object)model.volumen_estimado ?? DBNull.Value),
                new SqlParameter("@Precio_Unitario", (object)model.precio_unitario ?? DBNull.Value),
                new SqlParameter("@Dias_Asignados", model.dias_asignados),
                new SqlParameter("@Porcentaje_Adicional", (object)model.porcentaje_adicional ?? DBNull.Value));
        }

        public void EliminarServicioCotizado(int id)
        {
            AdoHelper.Execute("SP_ServiciosCotizados_Delete", CommandType.StoredProcedure,
                new SqlParameter("@ID", id));
        }
    }
}
