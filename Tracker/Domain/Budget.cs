using System;

namespace Tracker.Domain;

public class Budget;

public record DuplicateBudget(DateOnly SourceMonth, DateOnly TargetMonth);
