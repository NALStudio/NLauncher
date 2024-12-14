using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Code.Models;
public readonly struct CancellableResult<T>
{
    [MemberNotNullWhen(false, nameof(Value))]
    public bool WasCancelled { get; init; } = true;

    [MaybeNull]
    public T? Value => !WasCancelled ? value : throw new InvalidOperationException("Operation was cancelled.");
    private readonly T? value;

    private CancellableResult(bool cancelled, T? value, string? errorMsg)
    {
        WasCancelled = cancelled;
        this.value = value;
    }

    public static CancellableResult<T> Success(T value) => new(cancelled: false, value: value, errorMsg: null);
    public static CancellableResult<T> Cancelled() => new(cancelled: true, value: default, errorMsg: null);
}
