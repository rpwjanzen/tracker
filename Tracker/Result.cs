using System;

namespace Tracker;

public readonly struct ResultType<T, TError>: IEquatable<ResultType<T, TError>>
{
    private readonly T? _value;
    private readonly TError? _error;

    public T Value { get => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access value when failure."); }
    public TError Error { get => !IsSuccess ? _error! : throw new InvalidOperationException("Cannot access error when success."); }
    public bool IsSuccess { get; }
    
    public ResultType(bool isSuccess, T? value, TError? error) =>
        (IsSuccess, _value, _error) = (isSuccess, value, error);

    public bool Equals(ResultType<T, TError> other)
    {
        return this.IsSuccess == other.IsSuccess && (!this.IsSuccess || (this._value!.Equals(other._value)));
    }
}

static class Result
{
    public static ResultType<T, TError> Success<T, TError>(T value) =>
        new ResultType<T, TError>(true, value, default);

    public static ResultType<T, TError> Failure<T, TError>(TError error) =>
        new ResultType<T, TError>(false, default, error);

    public static ResultType<TResult, TError> Map<T, TResult, TError>(this ResultType<T, TError> result, Func<T, TResult> onSuccess)
        => result.IsSuccess ? Success<TResult, TError>(onSuccess(result.Value)) : Failure<TResult, TError>(result.Error);

    public static ResultType<T, TError2> MapError<T, TError, TError2>(this ResultType<T, TError> result, Func<TError, TError2> onError)
        => result.IsSuccess ? Success<T, TError2>(result.Value) : Failure<T, TError2>(onError(result.Error));

    public static TResult Match<T, TError, TResult>(
        this ResultType<T, TError> result,
        Func<T, TResult> onSuccess,
        Func<TError, TResult> onError
    ) => result.IsSuccess ? onSuccess(result.Value) : onError(result.Error);

    public static ResultType<TResult, TError> Bind<T, TResult, TError>(
        this ResultType<T, TError> result,
        Func<T, ResultType<TResult, TError>> bind)
        => result.IsSuccess ? bind(result.Value) : Failure<TResult, TError>(result.Error);
}