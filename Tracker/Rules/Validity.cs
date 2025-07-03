using System;
using System.Collections.Generic;
using System.Linq;
using Tracker.Domain;

namespace Tracker.Rules;

public interface IValidity<T>
{
    public bool IsSatisfiedBy(T item);
    public T ApplyTo(T item);
}

public class AllValidity<T>(IEnumerable<IValidity<T>> rules) : IValidity<T>
{
    // save to class variable to avoid double-enumeration
    private readonly List<IValidity<T>> _rules = [.. rules];

    public T ApplyTo(T item)
        => _rules.Aggregate(item, (result, rule) => rule.ApplyTo(result));

    public bool IsSatisfiedBy(T item)
        => _rules.All(rule => rule.IsSatisfiedBy(item));
}
//
// public sealed class AccountNameWhitespaceRule : IValidity<AccountName>
// {
//     public bool IsSatisfiedBy(AccountName accountName) =>
//         !accountName.Value.StartsWith(' ') &&
//         !accountName.Value.EndsWith(' ') &&
//         !accountName.Value.Contains('\t') &&
//         !accountName.Value.Contains('\n') &&
//         !accountName.Value.Contains('\r') &&
//         !accountName.Value.Contains("  ");
//
//     public AccountName ApplyTo(AccountName accountName)
//         => IsSatisfiedBy(accountName) ? accountName : new AccountName(Fix(accountName.Value));
//
//     private static string Fix(string value)
//         => string.Join(' ', value.Replace('\t', ' ')
//             .Replace('\n', ' ')
//             .Replace('\r', ' ')
//             .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
//     );
// }
//
// public sealed class NonEmptyNameRule : IValidity<AccountName>
// {
//     public bool IsSatisfiedBy(AccountName accountName)
//         => !string.IsNullOrWhiteSpace(accountName.Value);
//
//     public AccountName ApplyTo(AccountName accountName) => AccountName.Empty;
// }

