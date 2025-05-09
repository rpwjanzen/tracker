using System.Data;

namespace Tracker.Database;

// ReSharper disable TypeParameterCanBeVariant
public interface IDbQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    TResult Handle(TQuery query, IDbConnection connection);
}

public interface IDbCommandHandler<TCommand>
{
    public void Handle(TCommand command, IDbConnection connection);
}
// ReSharper restore TypeParameterCanBeVariant

public class DbCommandHandlerAdapter<T> : ICommandHandler<T>
{
    private readonly DapperContext _dapperContext;
    private readonly IDbCommandHandler<T> _commandHandler;
    
    public DbCommandHandlerAdapter(IDbCommandHandler<T> commandHandler, DapperContext dapperContext)
    {
        _commandHandler = commandHandler;
        _dapperContext = dapperContext;
    }

    public void Handle(T command)
    {
        using var connection = _dapperContext.CreateConnection();
        _commandHandler.Handle(command, connection);
    }
}

public class DbQueryHandlerAdapter<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly DapperContext _dapperContext;
    private readonly IDbQueryHandler<TQuery, TResult> _queryHandler;
    
    public DbQueryHandlerAdapter(IDbQueryHandler<TQuery, TResult> queryHandler, DapperContext dapperContext)
    {
        _queryHandler = queryHandler;
        _dapperContext = dapperContext;
    }
    
    public TResult Handle(TQuery query)
    {
        using var connection = _dapperContext.CreateConnection();
        return _queryHandler.Handle(query, connection);
    }
}
