using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Tracker.Database;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        // Reset();
    }

    public IDbConnection CreateConnection()
        => new SqliteConnection(_connectionString);

    public void Init()
    {
        using var connection = CreateConnection();
        connection.Open();
// long Id, string Name, decimal CurrentBalance, DateOnly BalanceDate, AccountKind Kind, BudgetKind BudgetKind
        var sql =
"""
CREATE TABLE IF NOT EXISTS accounts (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    kind TEXT NOT NULL,
    budgetKind TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS categories (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL
);

--INSERT INTO categories (id, name) VALUES(1, 'Giving');
--INSERT INTO categories (id, name) VALUES(2, 'Monthly Bills');
--INSERT INTO categories (id, name) VALUES(3, 'Everyday Expenses');
--INSERT INTO categories (id, name) VALUES(4, 'Rainy Day Funds');
--INSERT INTO categories (id, name) VALUES(5, 'Savings Goals');
--INSERT INTO categories (id, name) VALUES(6, 'Debt');

CREATE TABLE IF NOT EXISTS envelopes (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT ,
    month TEXT NOT NULL,
    amount numeric NOT NULL
);

CREATE TABLE IF NOT EXISTS financial_transactions (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    posted_on TEXT NOT NULL,
    payee TEXT NOT NULL,
    amount decimal,
    direction TEXT NOT NULL,
    memo TEXT NOT NULL,
    account_id INTEGER NOT NULL,
    cleared_status TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS financial_transactions_envelopes (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    financial_transaction_id INTEGER NOT NULL,
    envelope_id INTEGER NOT NULL,
    FOREIGN KEY (financial_transaction_id) REFERENCES financial_transactions(id),
    FOREIGN KEY (envelope_id) REFERENCES envelopes(id)
);

CREATE TABLE IF NOT EXISTS categories_envelopes (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    category_id INTEGER NOT NULL,
    envelope_id INTEGER NOT NULL,
    FOREIGN KEY (category_id) REFERENCES categories(id),
    FOREIGN KEY (envelope_id) REFERENCES envelopes(id)
);
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
""";
        connection.Execute(sql);
        connection.Close();
    }
    
    public void Import()
    {
        using var connection = CreateConnection();
        connection.Open();
        
        ImportCategories(connection);
        ImportEnvelopes(connection);

        connection.Close();
    }

    private static void ImportCategories(IDbConnection connection)
    {
        var lines = File.ReadLines("Import\\categories.csv");
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            connection.Execute(
                "INSERT INTO categories (id, name) VALUES (@id, @name)",
                new { id = parts[0], name = parts[1] }
            );
        }
    }

    private static void ImportEnvelopes(IDbConnection connection)
    {
        var lines = File.ReadLines("Import\\budgets.csv");
        foreach(var line in lines.Skip(1))
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