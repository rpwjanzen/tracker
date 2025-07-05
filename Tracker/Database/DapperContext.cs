using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Tracker.Database;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        SqlMapper.AddTypeHandler(new GuidHandler());
        SqlMapper.AddTypeHandler(new TimeSpanHandler());
        // SqlMapper.AddTypeHandler(new DecimalHandler());
        // Reset();
    }

    public SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        
        // enable foreign keys as they are not on by default
        conn.Execute("PRAGMA foreign_keys = ON; PRAGMA journal_mode = 'wal'");
        // MOAR performance
        conn.Execute("PRAGMA journal_mode = 'wal'");
        
        return conn;
    }
    
    public SqliteConnection CreateBulkInsertConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public void Init()
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
    account_type_id TEXT NOT NULL,
    budget_type_id TEXT NOT NULL,
    FOREIGN KEY (account_type_id) REFERENCES account_types(id),
    FOREIGN KEY (budget_type_id) REFERENCES budget_types(id)
) STRICT;

CREATE TABLE IF NOT EXISTS categories (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL
) STRICT;

CREATE TABLE IF NOT EXISTS envelopes (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT ,
    month TEXT NOT NULL,
    amount TEXT NOT NULL
) STRICT;

CREATE TABLE IF NOT EXISTS financial_transactions (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    posted_on TEXT NOT NULL,
    payee TEXT NOT NULL,
    amount TEXT NOT NULL,
    direction TEXT NOT NULL,
    memo TEXT NOT NULL,
    account_id INTEGER NOT NULL,
    cleared_status TEXT NOT NULL
) STRICT;

CREATE TABLE IF NOT EXISTS financial_transactions_envelopes (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    financial_transaction_id INTEGER NOT NULL,
    envelope_id INTEGER NOT NULL,
    FOREIGN KEY (financial_transaction_id) REFERENCES financial_transactions(id),
    FOREIGN KEY (envelope_id) REFERENCES envelopes(id)
) STRICT;

CREATE TABLE IF NOT EXISTS categories_envelopes (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    category_id INTEGER NOT NULL,
    envelope_id INTEGER NOT NULL,
    FOREIGN KEY (category_id) REFERENCES categories(id),
    FOREIGN KEY (envelope_id) REFERENCES envelopes(id)
) STRICT;
""";
        connection.Execute(sql);
        connection.Close();
    }

    public void Reset()
    {
        Clear();
        Init();
        Import();
    }

    public void Clear()
    {
        using var connection = CreateConnection();
        connection.Open();
        var sql =
"""
DROP TABLE IF EXISTS categories_envelopes;
DROP TABLE IF EXISTS financial_transactions_envelopes;
DROP TABLE IF EXISTS financial_transactions;
DROP TABLE IF EXISTS envelopes;
DROP TABLE IF EXISTS categories;
DROP TABLE IF EXISTS accounts;
DROP TABLE IF EXISTS account_types;
DROP TABLE IF EXISTS budget_types;
""";
        connection.Execute(sql);
        connection.Close();
    }

    public void Import()
    {
        using var connection = CreateBulkInsertConnection();

        ImportCsv(connection, "categories");
        // ImportEnvelopes(connection);
        // ImportCsv(connection, "account_types");
        // ImportCsv(connection, "budget_types");

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

    private static void ImportEnvelopes(IDbConnection connection)
    {
        var lines = File.ReadLines("Import\\envelopes.csv");
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            connection.Execute(
                "INSERT INTO envelopes (id, month, amount) VALUES (@id, @month, @amount)",
                new
                {
                    id = parts[0],
                    month = parts[1],
                    amount = parts[3]
                });

            connection.Execute(
                "INSERT INTO categories_envelopes (category_id, envelope_id) VALUES (@categoryId, @envelopeId)",
                new
                {
                    categoryId = parts[2],
                    envelopeId = parts[0]
                });
        }
    }
}