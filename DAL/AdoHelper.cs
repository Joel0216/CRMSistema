using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace CRMSistema.DAL
{
    /// <summary>
    /// Helper genérico para ejecutar consultas ADO.NET puro.
    /// Usa SqlConnection, SqlCommand y SqlDataReader para leer/escribir datos.
    /// </summary>
    public static class AdoHelper
    {
        public static List<dynamic> Query(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using (var con = Db.GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = commandType;
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                con.Open();
                using (var reader = cmd.ExecuteReader())
                    return MapToList(reader);
            }
        }

        public static List<T> Query<T>(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using (var con = Db.GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = commandType;
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                con.Open();
                using (var reader = cmd.ExecuteReader())
                    return MapToList<T>(reader);
            }
        }

        public static dynamic QuerySingle(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using (var con = Db.GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = commandType;
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToObject(reader);
                    return null;
                }
            }
        }

        public static T QuerySingle<T>(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using (var con = Db.GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = commandType;
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToObject<T>(reader);
                    return default;
                }
            }
        }

        public static int Execute(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using (var con = Db.GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = commandType;
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // ─────────────────────────────────────────────────────────
        // Mapeo a ExpandoObject (dynamic)
        // ─────────────────────────────────────────────────────────
        private static List<dynamic> MapToList(SqlDataReader reader)
        {
            var list = new List<dynamic>();
            while (reader.Read())
                list.Add(MapToObject(reader));
            return list;
        }

        private static dynamic MapToObject(SqlDataReader reader)
        {
            var item = new ExpandoObject() as IDictionary<string, object>;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                item[name] = value;
            }
            return item;
        }

        // ─────────────────────────────────────────────────────────
        // Mapeo a tipo fuerte
        // ─────────────────────────────────────────────────────────
        private static List<T> MapToList<T>(SqlDataReader reader)
        {
            var list = new List<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            while (reader.Read())
                list.Add(MapToObject<T>(reader, props));
            return list;
        }

        private static T MapToObject<T>(SqlDataReader reader, Dictionary<string, PropertyInfo> props = null)
        {
            props = props ?? typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var item = Activator.CreateInstance<T>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                if (props.TryGetValue(name, out var prop) && !reader.IsDBNull(i))
                {
                    var value = reader.GetValue(i);
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    prop.SetValue(item, Convert.ChangeType(value, targetType));
                }
            }
            return item;
        }
    }
}
