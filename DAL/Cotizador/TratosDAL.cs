using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CRMSistema.Models.Cotizador;
using Newtonsoft.Json.Linq;

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

        /// <summary>
        /// Crea o recupera el trato asociado a una cotización (identificado por folio) y
        /// persiste los servicios cotizados a partir del JSON del borrador/validación.
        /// Devuelve el Trato_ID y el total mensual calculado (subtotal + IVA).
        /// </summary>
        public int SincronizarTratoDesdeCotizacion(int prospectoId, string folio, string datosJson, out decimal totalMensual)
        {
            totalMensual = 0;
            var tratos = ObtenerPorProspecto(prospectoId);
            var trato = tratos?.FirstOrDefault(t =>
                (t.Nombre_Trato ?? "").Equals(folio, StringComparison.OrdinalIgnoreCase));

            // Si el trato existe pero tiene ID inválido (<=0), tratarlo como si no existiera.
            if (trato != null && trato.Trato_ID <= 0)
                trato = null;

            int tratoId;
            if (trato == null)
            {
                var servicios = new List<ServicioCotizadoModel>();
                decimal subtotal;
                ExtraerServiciosYSubtotal(datosJson, 0, out subtotal, out servicios);
                totalMensual = subtotal * 1.16m;

                tratoId = Crear(new TratoModel
                {
                    prospecto_id = prospectoId,
                    nombre_trato = folio,
                    importe = totalMensual,
                    fase_id = 2
                });

                if (tratoId > 0)
                {
                    foreach (var s in servicios)
                    {
                        s.trato_id = tratoId;
                        new CotizacionesDAL().CrearServicioCotizado(s);
                    }
                }
            }
            else
            {
                tratoId = trato.Trato_ID;
                var existentes = new CotizacionesDAL().ObtenerServiciosCotizados(tratoId);
                if (existentes == null || existentes.Count == 0)
                {
                    var servicios = new List<ServicioCotizadoModel>();
                    decimal subtotal;
                    ExtraerServiciosYSubtotal(datosJson, tratoId, out subtotal, out servicios);
                    foreach (var s in servicios)
                    {
                        new CotizacionesDAL().CrearServicioCotizado(s);
                    }
                    totalMensual = subtotal * 1.16m;
                }
                else
                {
                    totalMensual = (trato.Importe ?? 0);
                }
            }

            return tratoId;
        }

        private void ExtraerServiciosYSubtotal(string datosJson, int tratoId, out decimal subtotal, out List<ServicioCotizadoModel> servicios)
        {
            subtotal = 0;
            servicios = new List<ServicioCotizadoModel>();
            try
            {
                var datos = JObject.Parse(datosJson ?? "{}");
                var items = datos["items"] as JArray;
                if (items == null) return;

                foreach (var it in items)
                {
                    var dias = ExtraerDias(it["dias_asignados"]);
                    int diasPorSemana = dias.Count > 0 ? dias.Count : 1;
                    decimal adicional = (it["porcentaje_adicional"]?.Value<decimal>() ?? 0) / 100m;
                    decimal descuento = (it["porcentaje_descuento"]?.Value<decimal>() ?? 0) / 100m;

                    string tipo = (it["tipo_servicio"]?.Value<string>() ?? "").ToUpperInvariant();
                    decimal baseCalc = 0;
                    int? volumen = null;
                    decimal? precioUnitario = null;
                    int? recolectores = null;

                    if (tipo == "RSU")
                    {
                        int bolsas = it["bolsas"]?.Value<int>() ?? 0;
                        decimal bolsasMensuales = (bolsas * diasPorSemana * 52m) / 12m;
                        baseCalc = bolsasMensuales * 18.60m;
                        volumen = bolsas;
                        precioUnitario = 18.60m;
                    }
                    else
                    {
                        int rec = it["recolectores"]?.Value<int>() ?? 0;
                        decimal viajesMensuales = rec * diasPorSemana * 4m;
                        decimal costoT = it["costo_tonelada"]?.Value<decimal>() ?? 0;
                        decimal costoD = it["costo_disposicion"]?.Value<decimal>() ?? 0;
                        baseCalc = viajesMensuales * (costoT + costoD);
                        volumen = rec;
                        precioUnitario = costoT + costoD;
                        recolectores = rec;
                    }

                    decimal itemSubtotal = baseCalc * (1m + adicional) * (1m - descuento);
                    subtotal += itemSubtotal;

                    servicios.Add(new ServicioCotizadoModel
                    {
                        trato_id = tratoId,
                        tipo_residuo = it["tipo_residuo"]?.Value<string>() ?? tipo,
                        frecuencia = "Semanal",
                        periodicidad_pago = "Mensual",
                        volumen_estimado = volumen,
                        precio_unitario = precioUnitario,
                        dias_asignados = string.Join(",", dias),
                        porcentaje_adicional = it["porcentaje_adicional"]?.Value<decimal>(),
                        porcentaje_descuento = it["porcentaje_descuento"]?.Value<decimal>(),
                        sucursal_id = it["tab_id"]?.Value<string>(),
                        tipo_unidad = it["tipo_unidad"]?.Value<string>(),
                        tipo_cobro = it["tipo_cobro"]?.Value<string>(),
                        recolectores = recolectores,
                        turno = it["turno"]?.Value<string>(),
                        ruta = it["ruta"]?.Value<string>() ?? "Por asignar",
                        costo_tonelada = it["costo_tonelada"]?.Value<decimal>(),
                        costo_disposicion = it["costo_disposicion"]?.Value<decimal>()
                    });
                }
            }
            catch { }
        }

        private static List<string> ExtraerDias(JToken token)
        {
            var dias = new List<string>();
            if (token == null) return dias;
            if (token.Type == JTokenType.Array)
            {
                dias = token.ToObject<List<string>>() ?? new List<string>();
            }
            else
            {
                dias = token.ToString()
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrEmpty(d))
                    .ToList();
            }
            return dias;
        }
    }
}
