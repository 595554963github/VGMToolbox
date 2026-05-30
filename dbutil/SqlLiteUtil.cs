using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Text;

namespace VGMToolbox.dbutil
{
    public sealed class SqlLiteUtil
    {
        private SqlLiteUtil() { }

        public static DataTable GetSimpleDataTable(string databasePath,
            string tableName, string orderByField)
        {
            StringBuilder sqlCommand = new StringBuilder();

            DataTable dt = null;

            try
            {
                using (var conn = new SqliteConnection(String.Format(CultureInfo.InvariantCulture, "Data Source={0};Mode=ReadOnly", databasePath)))
                {
                    conn.Open();

                    sqlCommand.AppendFormat(CultureInfo.InvariantCulture, "SELECT * FROM {0}", tableName);

                    if (!String.IsNullOrEmpty(orderByField))
                    {
                        sqlCommand.AppendFormat(CultureInfo.InvariantCulture, " ORDER BY {0}", orderByField);
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sqlCommand.ToString();

                        using (var reader = cmd.ExecuteReader())
                        {
                            dt = new DataTable();
                            dt.Load(reader);
                            dt.Locale = CultureInfo.InvariantCulture;
                        }
                    }
                }

                return dt;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static DataTable GetSimpleDataItem(string databasePath,
            string tableName, string itemField, string itemId)
        {
            DataTable dt = null;

            try
            {
                using (var conn = new SqliteConnection(String.Format(CultureInfo.InvariantCulture,
                    "Data Source={0};Mode=ReadOnly", databasePath)))
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = String.Format(CultureInfo.InvariantCulture,
                            "SELECT * FROM {0} WHERE {1} = @id",
                            tableName, itemField);
                        cmd.Parameters.AddWithValue("@id", itemId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            dt = new DataTable();
                            dt.Load(reader);
                            dt.Locale = CultureInfo.InvariantCulture;
                        }
                    }
                }

                return dt;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
