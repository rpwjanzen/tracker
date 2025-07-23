using System;
using System.Data;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

// TODO: Determine if I need these workarounds?
// TODO: See https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/types

// Work around some Dapper + SQLite limitations 
// https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/dapper-limitations
// Dapper only ever returns: long, double, string, or byte[] by default. Add some more options.
internal abstract class SqliteTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    // Parameters are converted by Microsoft.Data.Sqlite
    public override void SetValue(IDbDataParameter parameter, T? value)
        => parameter.Value = value;
}

// TODO: create and use Span<char> parsing methods to avoid creating extra garbage

internal class DateTimeOffsetHandler : SqliteTypeHandler<DateTimeOffset>
{
    public override DateTimeOffset Parse(object value)
        => DateTimeOffset.Parse((string)value);
}

internal class DateOnlyHandler : SqliteTypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
        => DateOnly.Parse((string)value);
}

internal class GuidHandler : SqliteTypeHandler<Guid>
{
    public override Guid Parse(object value)
        => Guid.Parse((string)value);
}

internal class TimeSpanHandler : SqliteTypeHandler<TimeSpan>
{
    public override TimeSpan Parse(object value)
        => TimeSpan.Parse((string)value);
}

internal class YearMonthHandler : SqliteTypeHandler<YearMonth>
{
    public override YearMonth Parse(object value)
        => YearMonth.Parse((string)value);

    public override void SetValue(IDbDataParameter parameter, YearMonth value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString();
    }
}


// internal class DecimalHandler : SqliteTypeHandler<decimal>
// {
//     public override decimal Parse(object value)
//     {
//         if (value is long l)
//         {
//             return l;
//         } else if (value is double d)
//         {
//             return (decimal)d;
//         } else if (value is string s)
//         {
//             return decimal.Parse(s);
//         }
//         else if (value is byte[] bs)
//         {
//             throw new NotSupportedException("Cannot convert byte array to decimal.");
//         }
//         else if (value is null)
//         {
//             throw new NotSupportedException("Cannot convert 'null' to decimal.");
//         } else
//         {
//             // this should never happen
//             var typeName = value.GetType().Name;
//             throw new NotSupportedException($"Cannot convert {typeName} to decimal.");
//         }
//     }
// }
