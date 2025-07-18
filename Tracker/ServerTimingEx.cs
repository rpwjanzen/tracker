using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Lib.ServerTiming;

public static class ServerTimingEx
{
    public static void AddMetric(
        this IServerTiming serverTiming,
        TimeSpan duration,
        string? metricName = null,
        [CallerMemberName] string? functionName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        serverTiming.AddMetric(duration.Milliseconds, metricName, functionName, filePath, lineNumber);
    }
}

public class ServerTimingDbConnection(IDbConnection dbConnection, IServerTiming serverTiming) : IDbConnection
{
    public void Dispose()
    {
        dbConnection.Dispose();
    }

    public IDbTransaction BeginTransaction()
    {
        var transaction = dbConnection.BeginTransaction();
        return transaction;
    }

    public IDbTransaction BeginTransaction(IsolationLevel il)
    {
        var transaction = dbConnection.BeginTransaction(il);
        return transaction;
    }

    public void ChangeDatabase(string databaseName)
    {
        dbConnection.ChangeDatabase(databaseName);
    }

    public void Close()
    {
        dbConnection.Close();
    }

    public IDbCommand CreateCommand()
    {
        var command = new ServerTimingDbCommand(dbConnection.CreateCommand(), serverTiming);
        return command;
    }

    public void Open()
    {
        dbConnection.Open();
    }

    [AllowNull] public string ConnectionString
    {
        get => dbConnection.ConnectionString;
        set => dbConnection.ConnectionString = value;
    }

    public int ConnectionTimeout => dbConnection.ConnectionTimeout;

    public string Database => dbConnection.Database;

    public ConnectionState State => dbConnection.State;
}

public class ServerTimingDbCommand(IDbCommand dbCommand, IServerTiming serverTiming) : IDbCommand
{
    public void Dispose()
    {
        dbCommand.Dispose();
    }

    public void Cancel()
    {
        dbCommand.Cancel();
    }

    public IDbDataParameter CreateParameter()
    {
        var parameter = dbCommand.CreateParameter();
        return parameter;
    }

    public int ExecuteNonQuery()
    {
        var start = Stopwatch.GetTimestamp();
        var result = dbCommand.ExecuteNonQuery();
        var duration = Stopwatch.GetElapsedTime(start);
        if (duration.Milliseconds > 0)
        {
            serverTiming.AddMetric(duration, "sql");
        }

        return result;
    }

    public IDataReader ExecuteReader()
    {
        var start = Stopwatch.GetTimestamp();
        var reader = dbCommand.ExecuteReader();
        var duration = Stopwatch.GetElapsedTime(start);
        if (duration.Milliseconds > 0)
        {
            serverTiming.AddMetric(duration, "sql");
        }
        return reader;
    }

    public IDataReader ExecuteReader(CommandBehavior behavior)
    {
        var start = Stopwatch.GetTimestamp();
        var reader = dbCommand.ExecuteReader(behavior);
        var duration = Stopwatch.GetElapsedTime(start);
        if (duration.Milliseconds > 0)
        {
            serverTiming.AddMetric(duration, "sql");
        }
        return reader;
    }

    public object? ExecuteScalar()
    {
        var start = Stopwatch.GetTimestamp();
        var obj = dbCommand.ExecuteScalar();
        var duration = Stopwatch.GetElapsedTime(start);
        if (duration.Milliseconds > 0)
        {
            serverTiming.AddMetric(duration, "sql");
        }
        return obj;
    }

    public void Prepare()
    {
        dbCommand.Prepare();
    }

    [AllowNull] public string CommandText
    {
        get => dbCommand.CommandText;
        set => dbCommand.CommandText = value;
    }

    public int CommandTimeout
    {
        get => dbCommand.CommandTimeout;
        set => dbCommand.CommandTimeout = value;
    }

    public CommandType CommandType
    {
        get => dbCommand.CommandType;
        set => dbCommand.CommandType = value;
    }

    public IDbConnection? Connection
    {
        get => dbCommand.Connection;
        set => dbCommand.Connection = value;
    }

    public IDataParameterCollection Parameters => dbCommand.Parameters;

    public IDbTransaction? Transaction
    {
        get => dbCommand.Transaction;
        set => dbCommand.Transaction = value;
    }

    public UpdateRowSource UpdatedRowSource
    {
        get => dbCommand.UpdatedRowSource;
        set => dbCommand.UpdatedRowSource = value;
    }
}