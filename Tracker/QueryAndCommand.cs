// ReSharper disable TypeParameterCanBeVariant
// disable Co/Contra as per SimpleInjector recommendations at https://docs.simpleinjector.org/en/latest/advanced.html#covariance-and-contravariance
namespace Tracker;

// From https://blogs.cuttingedge.it/steven/posts/2011/meanwhile-on-the-query-side-of-my-architecture/

public interface IQuery<TResult>;

public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    TResult Handle(TQuery query);
}

// Optional, used if a controller depends on a bunch of different queries
public interface IQueryProcessor
{
    TResult Process<TResult>(IQuery<TResult> query);
}

public interface ICommandHandler<TCommand>
{
    public void Handle(TCommand command);
}