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
    }

    public IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public void Init()
    {
        using var connection = CreateConnection();
        connection.Open();
        var sql =
"""
CREATE TABLE IF NOT EXISTS accounts (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT,
    category INTEGER NOT NULL
);
INSERT INTO accounts (id, name, category) VALUES (1, 'Primary Chequing', 1);
INSERT INTO accounts (id, name, category) VALUES (2, 'Primary Savings', 1);
INSERT INTO accounts (id, name, category) VALUES (3, 'Line of Credit', 2);

CREATE TABLE IF NOT EXISTS categories (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, name TEXT);
INSERT INTO categories (id, name) VALUES(1, 'Groceries');
INSERT INTO categories (id, name) VALUES(2, 'Utilities');
INSERT INTO categories (id, name) VALUES(3, 'Housing');
INSERT INTO categories (id, name) VALUES(4, 'Transportation');

CREATE TABLE IF NOT EXISTS financial_transactions (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    posted_on TEXT,
    description TEXT,
    amount numeric,
    account_id INTEGER,
    category_id INTEGER
);
""";
        connection.Execute(sql);
        connection.Close();
    }

    public void Reset()
    {
        using var connection = CreateConnection();
        connection.Open();
        var sql =
"""
DROP TABLE IF EXISTS financial_transactions;
DROP TABLE IF EXISTS categories;
DROP TABLE IF EXISTS accounts;
DROP TABLE IF EXISTS account_types;
""";
        connection.Execute(sql);
        connection.Close();
    }
}