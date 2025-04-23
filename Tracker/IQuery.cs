namespace Tracker;

// From https://blogs.cuttingedge.it/steven/posts/2011/meanwhile-on-the-query-side-of-my-architecture/

public interface IQuery<TResult> { }

public interface IQueryHandler<in TQuery, out TResult>
    where TQuery : IQuery<TResult>
{
    TResult Handle(TQuery query);
}

// Optional, used if a controller depends on a bunch of different queries
public interface IQueryProcessor
{
    TResult Process<TResult>(IQuery<TResult> query);
}

public interface ICommandHandler<in TCommand>
{
    public void Handle(TCommand command);
}