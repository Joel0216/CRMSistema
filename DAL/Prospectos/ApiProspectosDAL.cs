using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using CRMSistema.Models.Prospectos;

namespace CRMSistema.DAL.Prospectos
{
    /// <summary>
    /// Acceso a datos para la API REST de prospectos.
    /// </summary>
    public class ApiProspectosDAL
    {
        public List<dynamic> ObtenerTodos()
        {
            var rows = AdoHelper.Query("SP_Prospectos_GetAll", CommandType.StoredProcedure);
            // Normalizar claves para que los consumidores (Razor, JS, controllers) puedan usar
            // nombres en camelCase sin depender del casing exacto de las columnas del SP.
            return rows.Select(NormalizarClaves).ToList();
        }

        private static dynamic NormalizarClaves(dynamic r)
        {
            var dict = r as IDictionary<string, object>;
            if (dict == null) return r;

            var result = new ExpandoObject() as IDictionary<string, object>;
            foreach (var kvp in dict)
                result[kvp.Key] = kvp.Value;

            void Alias(string nuevo, params string[] originales)
            {
                if (result.ContainsKey(nuevo)) return;
                foreach (var orig in originales)
                {
                    var key = dict.Keys.FirstOrDefault(k => k.Equals(orig, StringComparison.OrdinalIgnoreCase));
                    if (key != null)
                    {
                        result[nuevo] = dict[key];
                        return;
                    }
                }
            }

            Alias("id", "Prospecto_ID", "prospecto_id", "ProspectoId");
            Alias("nombre", "Nombre_Prospecto", "Nombre_Empresa", "Nombre_Comercial_Empresa", "Nombre_Comercial", "Empresa");
            Alias("contacto", "Nombre_Prospecto", "Contacto");
            Alias("rfc", "RFC");
            Alias("telefono", "Telefono");
            Alias("email", "Correo");
            Alias("estatus", "Estatus");
            Alias("tipoPersona", "Tipo_Persona", "TipoPersona");
            Alias("tieneSucursales", "Tiene_Sucursales", "TieneSucursales");
            Alias("nombreComercial", "Nombre_Comercial", "NombreComercial");
            Alias("calle", "Calle");
            Alias("numExt", "Num_Ext", "NumExt");
            Alias("numInt", "Num_Int", "NumInt");
            Alias("colonia", "Colonia");
            Alias("municipio", "Municipio");
            Alias("cp", "CP", "Cp");
            Alias("estado", "Estado");
            Alias("lat", "Lat");
            Alias("lng", "Lng");
            Alias("notas", "Notas");
            Alias("concesionaria", "Concesionaria");
            Alias("referencias", "Referencias");
            Alias("folioCatastral", "Folio_Catastral", "FolioCatastral");
            Alias("domicilioFiscal", "Domicilio_Fiscal", "DomicilioFiscal");
            Alias("domicilioRecoleccion", "Domicilio_Recoleccion", "DomicilioRecoleccion");
            Alias("dias_disponibles", "Dias_Disponibles", "DiasDisponibles");
            Alias("horario", "Horario");
            Alias("ruta", "Ruta");
            Alias("vendedorNombre", "VendedorNombre", "Nombre_Vendedor", "Vendedor");
            Alias("vendedorId", "Vendedor_ID", "VendedorId");
            Alias("foto_fachada", "Foto_Fachada");
            Alias("foto_acceso", "Foto_Acceso");
            Alias("foto_referencia", "Foto_Referencia");
            Alias("documento_catastral", "Documento_Catastral");
            Alias("documento_catastral_nombre", "Documento_Catastral_Nombre");
            Alias("motivoRechazo", "Motivo_Rechazo", "MotivoRechazo");

            return result;
        }

        public long UpsertEmpresa(string nombre, string rfc)
        {
            using (var db = Db.GetConnection())
            using (var cmd = new SqlCommand("SP_Empresa_Upsert", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@RFC", rfc ?? "");
                var pEmpId = new SqlParameter("@EmpresaID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pEmpId);

                db.Open();
                cmd.ExecuteNonQuery();
                return pEmpId.Value != null ? Convert.ToInt64(pEmpId.Value) : 0;
            }
        }

        public int Crear(ApiProspectoModel d, long empresaId, string contacto, string nombreTrim)
        {
            using (var db = Db.GetConnection())
            using (var cmd = new SqlCommand("SP_Prospectos_Create", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Empresa_ID", empresaId);
                cmd.Parameters.AddWithValue("@Nombre_Prospecto", contacto);
                cmd.Parameters.AddWithValue("@Nombre_Comercial_Empresa", nombreTrim);
                cmd.Parameters.AddWithValue("@Nombre_Comercial", d.nombreComercial);
                cmd.Parameters.AddWithValue("@Correo", d.email);
                cmd.Parameters.AddWithValue("@Telefono", d.telefono);
                cmd.Parameters.AddWithValue("@Tipo_Persona", d.tipoPersona ?? "Moral");
                cmd.Parameters.AddWithValue("@Tiene_Sucursales", d.tieneSucursales ?? "No");
                cmd.Parameters.AddWithValue("@Estatus", d.estatus ?? "Nuevo");
                cmd.Parameters.AddWithValue("@Tipo_Inmueble", d.tipoInmueble);
                cmd.Parameters.AddWithValue("@Notas", d.notas);
                cmd.Parameters.AddWithValue("@Calle", d.calle);
                cmd.Parameters.AddWithValue("@Num_Ext", d.numExt);
                cmd.Parameters.AddWithValue("@Num_Int", d.numInt);
                cmd.Parameters.AddWithValue("@Colonia", d.colonia);
                cmd.Parameters.AddWithValue("@Municipio", d.municipio);
                cmd.Parameters.AddWithValue("@CP", d.cp);
                cmd.Parameters.AddWithValue("@Estado", d.estado);
                cmd.Parameters.AddWithValue("@Concesionaria", d.concesionaria);
                cmd.Parameters.AddWithValue("@Referencias", d.referencias);
                cmd.Parameters.AddWithValue("@Folio_Catastral", d.folioCatastral);
                cmd.Parameters.AddWithValue("@Dias_Disponibles", d.dias_disponibles);
                cmd.Parameters.AddWithValue("@Horario", d.horario);
                cmd.Parameters.AddWithValue("@Ruta", d.ruta);
                cmd.Parameters.AddWithValue("@Lat", (object)d.lat ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Lng", (object)d.lng ?? DBNull.Value);
                cmd.Parameters.Add(new SqlParameter("@Foto_Comprobante", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(d.foto_comprobante) ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Foto_Fachada", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(d.foto_fachada) ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Foto_Acceso", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(d.foto_acceso) ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Foto_Referencia", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(d.foto_referencia) ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Documento_Catastral", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(d.documento_catastral) ?? DBNull.Value });
                cmd.Parameters.AddWithValue("@Documento_Catastral_Nombre", d.documento_catastral_nombre);

                db.Open();
                var result = cmd.ExecuteScalar();
                int nuevoId = result != null ? Convert.ToInt32(result) : 0;

                // Fallback: si el SP no devuelve el ID (devuelve 0), recuperarlo mediante SP.
                if (nuevoId == 0)
                {
                    var fallback = AdoHelper.QuerySingle("SP_Prospectos_GetIdByRfcCorreo", CommandType.StoredProcedure,
                        new SqlParameter("@RFC", d.rfc ?? ""),
                        new SqlParameter("@Correo", d.email ?? ""));
                    if (fallback != null) nuevoId = fallback.id != null ? Convert.ToInt32(fallback.id) : 0;
                }
                return nuevoId;
            }
        }

        public void Actualizar(int id, ApiProspectoModel d, string contacto, string nombreTrim)
        {
            using (var db = Db.GetConnection())
            using (var cmd = new SqlCommand("SP_Empresa_UpdateByProspecto", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Prospecto_ID", id);
                cmd.Parameters.AddWithValue("@Nombre_Empresa", nombreTrim);
                cmd.Parameters.AddWithValue("@RFC", d.rfc ?? "");
                db.Open();
                cmd.ExecuteNonQuery();
            }

            using (var db = Db.GetConnection())
            using (var cmd = new SqlCommand("SP_Prospectos_Update", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Prospecto_ID", id);
                cmd.Parameters.AddWithValue("@Nombre_Prospecto", contacto);
                cmd.Parameters.AddWithValue("@Nombre_Comercial_Empresa", nombreTrim);
                cmd.Parameters.AddWithValue("@Nombre_Comercial", d.nombreComercial);
                cmd.Parameters.AddWithValue("@Correo", d.email);
                cmd.Parameters.AddWithValue("@Telefono", d.telefono);
                cmd.Parameters.AddWithValue("@Tipo_Persona", d.tipoPersona ?? "Moral");
                cmd.Parameters.AddWithValue("@Tiene_Sucursales", d.tieneSucursales ?? "No");
                cmd.Parameters.AddWithValue("@Estatus", d.estatus ?? "Nuevo");
                cmd.Parameters.AddWithValue("@Tipo_Inmueble", d.tipoInmueble);
                cmd.Parameters.AddWithValue("@Notas", d.notas);
                cmd.Parameters.AddWithValue("@Calle", d.calle);
                cmd.Parameters.AddWithValue("@Num_Ext", d.numExt);
                cmd.Parameters.AddWithValue("@Num_Int", d.numInt);
                cmd.Parameters.AddWithValue("@Colonia", d.colonia);
                cmd.Parameters.AddWithValue("@Municipio", d.municipio);
                cmd.Parameters.AddWithValue("@CP", d.cp);
                cmd.Parameters.AddWithValue("@Estado", d.estado);
                cmd.Parameters.AddWithValue("@Concesionaria", d.concesionaria);
                cmd.Parameters.AddWithValue("@Referencias", d.referencias);
                cmd.Parameters.AddWithValue("@Folio_Catastral", d.folioCatastral);
                cmd.Parameters.AddWithValue("@Dias_Disponibles", d.dias_disponibles);
                cmd.Parameters.AddWithValue("@Horario", d.horario);
                cmd.Parameters.AddWithValue("@Ruta", d.ruta);
                cmd.Parameters.Add(new SqlParameter("@Foto_Comprobante", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(d.foto_comprobante) ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Foto_Fachada", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(d.foto_fachada) ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Foto_Acceso", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(d.foto_acceso) ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Foto_Referencia", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(d.foto_referencia) ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Documento_Catastral", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(d.documento_catastral) ?? DBNull.Value });
                cmd.Parameters.AddWithValue("@Documento_Catastral_Nombre", d.documento_catastral_nombre);
                db.Open();
                cmd.ExecuteNonQuery();
            }

            AdoHelper.Execute("SP_ProspectoSucursales_DeleteByProspecto", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id));
            InsertarSucursales(id, d.sucursales);

            AdoHelper.Execute("SP_ProspectoContactos_DeleteByProspecto", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id));
            InsertarContactos(id, d.contactos);
        }

        /// <summary>
        /// Actualiza solo datos básicos del prospecto y el contacto representante
        /// sin borrar sucursales ni otros contactos. Usado desde edición de contratos.
        /// </summary>
        /// <param name="actualizarDireccionEstructurada">Si es true, también escribe Calle/Num_Ext/etc.</param>
        public void ActualizarBasicoDesdeContrato(int id, ApiProspectoModel d, string representanteLegal, bool actualizarDireccionEstructurada = false)
        {
            using (var db = Db.GetConnection())
            using (var cmd = new SqlCommand("SP_Empresa_UpdateByProspecto", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Prospecto_ID", id);
                cmd.Parameters.AddWithValue("@Nombre_Empresa", d.nombre ?? "");
                cmd.Parameters.AddWithValue("@RFC", d.rfc ?? "");
                db.Open();
                cmd.ExecuteNonQuery();
            }

            AdoHelper.Execute("SP_Prospectos_UpdateBasicoDesdeContrato", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id),
                new SqlParameter("@Nombre_Prospecto", representanteLegal ?? d.contacto ?? ""),
                new SqlParameter("@Nombre_Comercial_Empresa", d.nombre ?? ""),
                new SqlParameter("@Nombre_Comercial", d.nombreComercial ?? ""),
                new SqlParameter("@RFC", d.rfc ?? ""),
                new SqlParameter("@Correo", d.email ?? ""),
                new SqlParameter("@Telefono", d.telefono ?? ""),
                new SqlParameter("@Folio_Catastral", d.folioCatastral ?? ""),
                new SqlParameter("@Domicilio_Fiscal", d.domicilioFiscal ?? ""),
                new SqlParameter("@Domicilio_Recoleccion", d.domicilioRecoleccion ?? ""),
                new SqlParameter("@ActualizarDireccion", actualizarDireccionEstructurada ? 1 : 0),
                new SqlParameter("@Calle", d.calle ?? ""),
                new SqlParameter("@Num_Ext", d.numExt ?? ""),
                new SqlParameter("@Num_Int", d.numInt ?? ""),
                new SqlParameter("@Colonia", d.colonia ?? ""),
                new SqlParameter("@Municipio", d.municipio ?? ""),
                new SqlParameter("@CP", d.cp ?? ""),
                new SqlParameter("@Estado", d.estado ?? ""));

            if (!string.IsNullOrWhiteSpace(representanteLegal))
            {
                AdoHelper.Execute("SP_ProspectoContactos_UpsertRepresentanteLegal", CommandType.StoredProcedure,
                    new SqlParameter("@Prospecto_ID", id),
                    new SqlParameter("@Nombre_Contacto", representanteLegal),
                    new SqlParameter("@Correo", d.email ?? ""),
                    new SqlParameter("@Telefono", d.telefono ?? ""));
            }
        }

        /// <summary>
        /// Actualiza domicilios, folio catastral y archivos (fotos/carta) de un prospecto
        /// sin tocar contactos ni sucursales. Los archivos nulos conservan el valor actual.
        /// </summary>
        public void ActualizarArchivosYFolio(int id, string domicilioFiscal, string domicilioRecoleccion, string folioCatastral,
            byte[] fotoFachada, byte[] fotoAcceso, byte[] fotoReferencia, byte[] documentoCatastral, string documentoCatastralNombre)
        {
            AdoHelper.Execute("SP_Prospectos_UpdateArchivosYFolio", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id),
                new SqlParameter("@Domicilio_Fiscal", domicilioFiscal ?? ""),
                new SqlParameter("@Domicilio_Recoleccion", domicilioRecoleccion ?? ""),
                new SqlParameter("@Folio_Catastral", folioCatastral ?? ""),
                new SqlParameter("@Foto_Fachada", SqlDbType.VarBinary, -1) { Value = (object)fotoFachada ?? DBNull.Value },
                new SqlParameter("@Foto_Acceso", SqlDbType.VarBinary, -1) { Value = (object)fotoAcceso ?? DBNull.Value },
                new SqlParameter("@Foto_Referencia", SqlDbType.VarBinary, -1) { Value = (object)fotoReferencia ?? DBNull.Value },
                new SqlParameter("@Documento_Catastral", SqlDbType.VarBinary, -1) { Value = (object)documentoCatastral ?? DBNull.Value },
                new SqlParameter("@Documento_Catastral_Nombre", documentoCatastralNombre ?? ""));
        }

        public void ActualizarEstatus(int id, string estatus)
        {
            AdoHelper.Execute("SP_Prospectos_UpdateEstatus", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id),
                new SqlParameter("@Estatus", estatus ?? ""));
        }

        public string AsignarVendedor(int id, int vendedorId)
        {
            using (var db = Db.GetConnection())
            using (var cmd = new SqlCommand("SP_Prospectos_AsignarVendedor", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Prospecto_ID", id);
                cmd.Parameters.AddWithValue("@Vendedor_ID", vendedorId);
                var pNombre = new SqlParameter("@NombreVendedor", SqlDbType.VarChar, 150) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pNombre);

                db.Open();
                cmd.ExecuteNonQuery();
                return pNombre.Value?.ToString() ?? "";
            }
        }

        public List<dynamic> ObtenerSucursales(int id)
        {
            return AdoHelper.Query("SP_ProspectoSucursales_GetByProspecto", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id));
        }

        public List<dynamic> ObtenerContactos(int id)
        {
            return AdoHelper.Query("SP_ProspectoContactos_GetByProspecto", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id));
        }

        public void Eliminar(int id)
        {
            AdoHelper.Execute("SP_Prospectos_Delete", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id));
        }

        public void Rechazar(int id, string motivo, int? rechazadoPor)
        {
            AdoHelper.Execute("SP_Prospectos_Rechazar", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id),
                new SqlParameter("@Motivo", motivo ?? "Sin motivo especificado"),
                new SqlParameter("@Rechazado_Por", (object)rechazadoPor ?? DBNull.Value));
        }

        public void InsertarNotificacion(int id, ApiNotificacionModel req, string passwordTemporal,
            string cotizacionRef, string vigenciaInicio, string vigenciaFin)
        {
            AdoHelper.Execute("SP_Notificaciones_Insert", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id),
                new SqlParameter("@Tipo_Asunto", req.tipo_asunto ?? ""),
                new SqlParameter("@Correo_Destino", req.correo_destino ?? ""),
                new SqlParameter("@Password_Temporal", (object)passwordTemporal ?? DBNull.Value),
                new SqlParameter("@Cotizacion_Ref", (object)cotizacionRef ?? DBNull.Value),
                new SqlParameter("@Vigencia_Inicio", (object)vigenciaInicio ?? DBNull.Value),
                new SqlParameter("@Vigencia_Fin", (object)vigenciaFin ?? DBNull.Value),
                new SqlParameter("@Enviado_Por", (object)req.enviado_por ?? DBNull.Value));
        }

        public List<dynamic> ObtenerNotificaciones(int id)
        {
            return AdoHelper.Query("SP_Notificaciones_GetByProspecto", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id));
        }

        public void InsertarArchivo(int id, ApiArchivoModel req)
        {
            byte[] buffer = Convert.FromBase64String(req.base64);
            string tipo = string.IsNullOrEmpty(req.tipo) ? "application/octet-stream" : req.tipo;
            int peso = req.peso > 0 ? req.peso : buffer.Length;

            AdoHelper.Execute("SP_ProspectoArchivos_Insert", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id),
                new SqlParameter("@archivo_binario", (object)buffer ?? DBNull.Value) { SqlDbType = SqlDbType.VarBinary, Size = -1 },
                new SqlParameter("@archivo_nombre", req.nombre),
                new SqlParameter("@archivo_peso", peso),
                new SqlParameter("@archivo_tipo", tipo));
        }

        public List<dynamic> ListarArchivos(int id)
        {
            return AdoHelper.Query("SP_ProspectoArchivos_GetByProspecto", CommandType.StoredProcedure,
                new SqlParameter("@Prospecto_ID", id));
        }

        public dynamic ObtenerArchivo(int archivoId)
        {
            return AdoHelper.QuerySingle("SP_ProspectoArchivos_GetById", CommandType.StoredProcedure,
                new SqlParameter("@Archivo_ID", archivoId));
        }

        public void EliminarArchivo(int archivoId)
        {
            AdoHelper.Execute("SP_ProspectoArchivos_Delete", CommandType.StoredProcedure,
                new SqlParameter("@Archivo_ID", archivoId));
        }

        public void InsertarSucursales(int prospectoId, List<ApiSucursalModel> sucursales)
        {
            if (sucursales == null || sucursales.Count == 0) return;
            foreach (var suc in sucursales)
            {
                AdoHelper.Execute("SP_ProspectoSucursales_Insert", CommandType.StoredProcedure,
                    new SqlParameter("@Prospecto_ID", prospectoId),
                    new SqlParameter("@Nombre_Sucursal", suc.nombre_sucursal ?? ""),
                    new SqlParameter("@Correo_Electronico", suc.correo_electronico ?? ""),
                    new SqlParameter("@Telefono_Sucursal", suc.telefono_sucursal ?? ""),
                    new SqlParameter("@Nombre_Responsable", suc.nombre_responsable ?? ""),
                    new SqlParameter("@Calle", suc.calle),
                    new SqlParameter("@Num_Ext", suc.numExt),
                    new SqlParameter("@Num_Int", suc.numInt),
                    new SqlParameter("@Colonia", suc.colonia),
                    new SqlParameter("@Municipio", suc.municipio),
                    new SqlParameter("@CP", suc.cp),
                    new SqlParameter("@Estado", suc.estado),
                    new SqlParameter("@Lat", (object)suc.lat ?? DBNull.Value),
                    new SqlParameter("@Lng", (object)suc.lng ?? DBNull.Value),
                    new SqlParameter("@Foto_Comprobante", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(suc.foto_comprobante) ?? DBNull.Value },
                    new SqlParameter("@Foto_Fachada", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(suc.foto_fachada) ?? DBNull.Value },
                    new SqlParameter("@Foto_Acceso", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(suc.foto_acceso) ?? DBNull.Value },
                    new SqlParameter("@Foto_Referencia", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(suc.foto_referencia) ?? DBNull.Value },
                    new SqlParameter("@Documento_Catastral", SqlDbType.VarBinary, -1) { Value = (object)ParseBase64(suc.documento_catastral) ?? DBNull.Value },
                    new SqlParameter("@Documento_Catastral_Nombre", suc.documento_catastral_nombre),
                    new SqlParameter("@Concesionaria", suc.concesionaria),
                    new SqlParameter("@Referencias", suc.referencias),
                    new SqlParameter("@Folio_Catastral", suc.folioCatastral));
            }
        }

        public void InsertarContactos(int prospectoId, List<ApiContactoModel> contactos)
        {
            if (contactos == null || contactos.Count == 0) return;
            foreach (var c in contactos)
            {
                AdoHelper.Execute("SP_ProspectoContactos_Insert", CommandType.StoredProcedure,
                    new SqlParameter("@Prospecto_ID", prospectoId),
                    new SqlParameter("@Nombre_Contacto", c.nombre_contacto ?? ""),
                    new SqlParameter("@Correo", c.correo),
                    new SqlParameter("@Representante_Legal", c.representante_legal),
                    new SqlParameter("@Telefono", c.telefono));
            }
        }

        private static byte[] ParseBase64(string base64Str)
        {
            if (string.IsNullOrEmpty(base64Str)) return null;
            try
            {
                var clean = base64Str.Contains(",") ? base64Str.Split(',')[1] : base64Str;
                return Convert.FromBase64String(clean);
            }
            catch { return null; }
        }
    }
}
