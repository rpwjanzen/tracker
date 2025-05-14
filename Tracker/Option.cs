using System.Diagnostics.CodeAnalysis;

namespace Tracker;

public readonly struct OptionType<T>: IEquatable<OptionType<T>>
{
    private readonly T? _value;
    public T Value { get => HasValue ? _value! : throw new InvalidOperationException("Cannot access value of none option."); }
    public bool HasValue { get; }
    
    public OptionType(bool hasValue, T? value) =>
        (HasValue, _value) = (hasValue, value);

    public static implicit operator OptionType<T>(T? value) => value is not null ? new OptionType<T>(true, value) : new OptionType<T>(false, default);

    public bool Equals(OptionType<T> other)
    {
        return this.HasValue == other.HasValue && (!this.HasValue || this._value!.Equals(other._value));
    }
}

public static class Option
{
    public static OptionType<T> Some<T>(T value) => new(true, value);
    public static OptionType<T> None<T>() => new(false, default);

    public static OptionType<TResult> Map<T, TResult>(this OptionType<T> option, Func<T, TResult> map)
        => option.HasValue ? Some(map(option.Value)) : None<TResult>();

    public static T Reduce<T>(this OptionType<T> option, T ifNone)
        => option.HasValue ? option.Value : ifNone;

    public static TResult Match<T, TResult>(this OptionType<T> option, Func<T, TResult> onSome, Func<TResult> onNone)
        => option.HasValue ? onSome(option.Value) : onNone();

    public static OptionType<T> ToOption<T>(this T? value) => value is not null ? Some(value) : None<T>();
}