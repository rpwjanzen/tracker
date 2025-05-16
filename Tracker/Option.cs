using System.Diagnostics.CodeAnalysis;

namespace Tracker;

public readonly struct OptionType<T>(T? value) : IEquatable<OptionType<T>>
{
    private readonly T? _value = value;
    public T Value { get => _value ?? throw new InvalidOperationException("Cannot access value of none option."); }
    public bool HasValue { get => _value is not null; }

    public static implicit operator OptionType<T>(T? value) => new(value);
    public static implicit operator T?(OptionType<T> value) => value._value;

    public override int GetHashCode() => _value?.GetHashCode() ?? 0;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is OptionType<T> other && Equals(other);
    public bool Equals(OptionType<T> other) => HasValue == other.HasValue && (!HasValue || _value!.Equals(other._value));

    public static bool operator ==(OptionType<T> left, OptionType<T> right) => left.Equals(right);
    public static bool operator !=(OptionType<T> left, OptionType<T> right) => !(left == right);
}

public static class Option
{
    public static OptionType<T> Some<T>(T value) => new(value);
    public static OptionType<T> None<T>() => new(default);

    public static OptionType<TResult> Map<T, TResult>(this OptionType<T> option, Func<T, TResult> map)
        => option.HasValue ? Some(map(option.Value)) : None<TResult>();

    public static T Reduce<T>(this OptionType<T> option, T ifNone)
        => option.HasValue ? option.Value : ifNone;

    public static T Reduce<T>(this OptionType<T> option, Func<T> ifNone)
    => option.HasValue ? option.Value : ifNone();

    public static TResult Match<T, TResult>(this OptionType<T> option, Func<T, TResult> onSome, Func<TResult> onNone)
        => option.HasValue ? onSome(option.Value) : onNone();

    public static OptionType<T> Bind<T>(this T? value) where T : class => value is not null ? Some(value) : None<T>();

    // public static OptionType<T> ToOption<T>(this T? value) where T : class => value is not null ? Some(value) : None<T>();
    public static OptionType<T> ToOption<T>(this T? value) where T : struct => value.HasValue ? Some(value.Value) : None<T>();
    public static OptionType<T> ToOption<T>(this T value) where T : struct => !value.Equals(default) ? Some(value) : None<T>();

    // public static OptionType<ValueTuple<T>> ToOption<T>(this ValueTuple<T> value) => value.Equals(default) ? Some(value) : None<ValueTuple<T>>();
    // public static OptionType<ValueTuple<T1, T2>> ToOption<T1, T2>(this ValueTuple<T1, T2> value) => value.Equals(default) ? Some(value) : None<ValueTuple<T1, T2>>();
    // public static OptionType<ValueTuple<T1, T2, T3>> ToOption<T1, T2, T3>(this ValueTuple<T1, T2, T3> value) => value.Equals(default) ? Some(value) : None<ValueTuple<T1, T2, T3>>();

    // public static OptionType<T> FromClass<T>(this T? value) where T : class => value is null ? None<T>() : Some(value);
    // public static OptionType<T> FromStruct<T>(this Nullable<T> value) where T : struct => value.HasValue ? Some(value.Value) : None<T>();
    // public static OptionType<long> FromStruct(this long? value) => value.HasValue ? Some(value.Value) : None<long>();

    // public static OptionType<T> FromNullable<T>(this T? value) where T : struct => value.HasValue ? Some(value.Value) : None<T>();
    public static T? ToNullable<T>(this OptionType<T> option) where T : struct => option.HasValue ? option.Value : null;
}