//Pablo Valente Neto

using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Data;

// Métodos de extensão para DbDataReader, simplificando a extração e conversão dos valores
// das colunas retornadas pelas consultas SQL para tipos nativos do C#,
// tratando conversões de tipo e checagem de valores nulos de forma padronizada.
public static class DataReaderExtensions
{
    public static string GetStringValue(this DbDataReader reader, string columnName)
    {
        return reader[columnName].ToString()!;
    }

    public static string GetNullableString(this DbDataReader reader, string columnName, string defaultValue = "")
    {
        return reader[columnName] is DBNull ? defaultValue : reader[columnName].ToString()!;
    }

    public static byte[]? GetNullableBytes(this DbDataReader reader, string columnName)
    {
        return reader[columnName] is DBNull ? null : (byte[])reader[columnName];
    }

    public static int GetInt32Value(this DbDataReader reader, string columnName)
    {
        return Convert.ToInt32(reader[columnName]);
    }

    public static DateTime GetDateTimeValue(this DbDataReader reader, string columnName)
    {
        return Convert.ToDateTime(reader[columnName]);
    }

    public static DateOnly GetDateOnlyValue(this DbDataReader reader, string columnName)
    {
        return DateOnly.FromDateTime(Convert.ToDateTime(reader[columnName]));
    }
}
