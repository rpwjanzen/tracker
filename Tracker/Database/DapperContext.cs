using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using Lib.ServerTiming;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Tracker.Database;

public class DapperContext
{
    private readonly string _connectionString;
    private readonly IServerTiming _serverTiming;

    public DapperContext(IConfiguration configuration, IServerTiming serverTiming)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _serverTiming = serverTiming;
        
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        SqlMapper.AddTypeHandler(new DateOnlyHandler());
        SqlMapper.AddTypeHandler(new GuidHandler());
        SqlMapper.AddTypeHandler(new TimeSpanHandler());
        SqlMapper.AddTypeHandler(new YearMonthHandler());
        // SqlMapper.AddTypeHandler(new DecimalHandler());
    }

    public IDbConnection CreateConnection()
    {
        // var conn = new ServerTimingDbConnection(new SqliteConnection(_connectionString), _serverTiming);
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        
        // enable foreign keys as they are not on by default
        // enable wal  for more performance: PRAGMA journal_mode = 'wal';
        // increase page size from defaults of 4096 for more performance
        // sync=off for more (dev) performance: PRAGMA synchronous = OFF;
        conn.Execute("PRAGMA foreign_keys = ON; PRAGMA page_size = 8192;");
        return conn;
    }
    
    public SqliteConnection CreateBulkInsertConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public void CreateSchema()
    {
        using var connection = CreateBulkInsertConnection();
        // Data types used are based off of MS info at
        // https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/types
        var sql =
"""
CREATE TABLE IF NOT EXISTS account_types (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL
) STRICT ;

CREATE TABLE IF NOT EXISTS budget_types (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL
) STRICT;

CREATE TABLE IF NOT EXISTS accounts (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    account_type_id INTEGER NOT NULL,
    budget_type_id INTEGER NOT NULL,
    FOREIGN KEY (account_type_id) REFERENCES  account_types(id),
    FOREIGN KEY (budget_type_id) REFERENCES budget_types(id)
) STRICT;

CREATE TABLE IF NOT EXISTS categories (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL
) STRICT;

CREATE TABLE IF NOT EXISTS envelopes (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    month TEXT NOT NULL,
    budgeted TEXT NOT NULL,
    category_id INTEGER NOT NULL,
     FOREIGN KEY (category_id) REFERENCES categories(id),
     UNIQUE (month, category_id)
) STRICT;

CREATE TABLE IF NOT EXISTS cleared_statuses (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL
) STRICT;

CREATE TABLE IF NOT EXISTS financial_transactions (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    posted_on TEXT NOT NULL,
    payee TEXT NOT NULL,
    amount TEXT NOT NULL,
    direction TEXT NOT NULL,
    memo TEXT NOT NULL,
    account_id INTEGER NOT NULL,
    cleared_status_id INTEGER NOT NULL,
    category_id INTEGER NOT NULL,
    FOREIGN KEY (account_id) REFERENCES accounts(id),
    FOREIGN KEY (cleared_status_id) REFERENCES cleared_statuses(id),
    FOREIGN KEY (category_id) REFERENCES categories(id)
) STRICT;
""";
        connection.Execute(sql);
        connection.Close();
    }

    public void Reset()
    {
        ClearSchema();
        CreateSchema();
        Import();
    }

    public void ClearSchema()
    {
        using var connection = CreateConnection();
        connection.Open();
        var sql =
"""
--DROP TABLE IF EXISTS financial_transactions;
DROP TABLE IF EXISTS envelopes;
--DROP TABLE IF EXISTS categories;
--DROP TABLE IF EXISTS accounts;
--DROP TABLE IF EXISTS account_types;
--DROP TABLE IF EXISTS budget_types;
--DROP TABLE IF EXISTS cleared_statuses;
""";
        connection.Execute(sql);
        connection.Close();
    }

    public void Import()
    {
        using var connection = CreateBulkInsertConnection();

        // ImportCsv(connection, "categories");
        // ImportCsv(connection, "account_types");
        // ImportCsv(connection, "budget_types");
        // ImportCsv(connection, "cleared_statuses");
        // ImportCsv(connection, "accounts");
        ImportCsv(connection, "envelopes");

        connection.Close();
    }
    
    private static void ImportCsv(IDbConnection connection, string tableName)
    {
        var parameters = new List<IDbDataParameter>();
        using var transaction = connection.BeginTransaction();
        var command = connection.CreateCommand();
        
        var isFirst = true;
        foreach (var line in File.ReadLines($"Import\\{tableName}.csv"))
        {
            if (isFirst)
            {
                isFirst = false;
                ProcessHeaders(tableName, line, command, parameters);
            }
            else
            {
                var parts = line.Split(',');
                for (var i =0; i < parts.Length; i++)
                {
                    parameters[i].Value = parts[i];
                }
                command.ExecuteNonQuery();
            }
        }
        transaction.Commit();
    }

    private static void ProcessHeaders(string tableName, string line, IDbCommand command, List<IDbDataParameter> parameters)
    {
        var headers = line.Split(',');
        foreach (var header in headers)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@" + header;
            command.Parameters.Add(parameter);
            parameters.Add(parameter);
        }

        // yes, we support SQL injection from CSV files.
        var keyNames = string.Join(',', headers);
        var keyParams = string.Join(',', headers.Select(x => '@' + x));
        command.CommandText = $"INSERT INTO {tableName} ({keyNames}) VALUES ({keyParams})";
    }
}