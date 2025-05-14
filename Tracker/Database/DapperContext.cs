using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
[module:DapperAot]

namespace Tracker.Database;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        // this.Reset();
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

CREATE TABLE IF NOT EXISTS categories (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, name TEXT, parent_id INTEGER NULL DEFAULT  NULL);
INSERT INTO categories (id, name) VALUES(1, 'Giving');
INSERT INTO categories (id, name) VALUES(2, 'Monthly Bills');
INSERT INTO categories (id, name) VALUES(3, 'Everyday Expenses');
INSERT INTO categories (id, name) VALUES(4, 'Rainy Day Funds');
INSERT INTO categories (id, name) VALUES(5, 'Savings Goals');
INSERT INTO categories (id, name) VALUES(6, 'Debt');

INSERT INTO categories (name, parent_id) VALUES('Tithing', 1);
INSERT INTO categories (name, parent_id) VALUES('Charitable', 1);

INSERT INTO categories (name, parent_id) VALUES('Mortgage', 2);
INSERT INTO categories (name, parent_id) VALUES('Phone', 2);
INSERT INTO categories (name, parent_id) VALUES('Internet', 2);
INSERT INTO categories (name, parent_id) VALUES('Cable TV', 2);
INSERT INTO categories (name, parent_id) VALUES('Electricity', 2);
INSERT INTO categories (name, parent_id) VALUES('Water', 2);
INSERT INTO categories (name, parent_id) VALUES('Natural Gas', 2);

INSERT INTO categories (name, parent_id) VALUES('Groceries', 3);
INSERT INTO categories (name, parent_id) VALUES('Fuel', 3);
INSERT INTO categories (name, parent_id) VALUES('Spending Money', 3);
INSERT INTO categories (name, parent_id) VALUES('Restaurants', 3);
INSERT INTO categories (name, parent_id) VALUES('Medical', 3);
INSERT INTO categories (name, parent_id) VALUES('Clothing', 3);
INSERT INTO categories (name, parent_id) VALUES('Household Goods', 3);

INSERT INTO categories (name, parent_id) VALUES('Emergency Fund', 4);
INSERT INTO categories (name, parent_id) VALUES('Car Repairs', 4);
INSERT INTO categories (name, parent_id) VALUES('Home Maintenance', 4);
INSERT INTO categories (name, parent_id) VALUES('Car Insurance', 4);
INSERT INTO categories (name, parent_id) VALUES('Life Insurance', 4);
INSERT INTO categories (name, parent_id) VALUES('Health Insurance', 4);
INSERT INTO categories (name, parent_id) VALUES('Birthdays', 4);
INSERT INTO categories (name, parent_id) VALUES('Christmas', 4);

INSERT INTO categories (name, parent_id) VALUES('Car Replacement', 5);
INSERT INTO categories (name, parent_id) VALUES('Vacation', 5);

INSERT INTO categories (name, parent_id) VALUES('Car Payment', 6);

CREATE TABLE IF NOT EXISTS envelopes (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT ,
    month TEXT,
    amount numeric,
    category_id INTEGER
);

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
        Clear();
        Init();
    }

    public void Clear()
    {
        using var connection = CreateConnection();
        connection.Open();
        var sql =
"""
DROP TABLE IF EXISTS financial_transactions;
DROP TABLE IF EXISTS envelopes;
DROP TABLE IF EXISTS categories;
DROP TABLE IF EXISTS accounts;
DROP TABLE IF EXISTS account_types;
""";
        connection.Execute(sql);
        connection.Close();
    }
}