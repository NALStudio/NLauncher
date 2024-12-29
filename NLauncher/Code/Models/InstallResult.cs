using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Code.Models;

public readonly struct InstallResult
{
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool IsSuccess { get; }
    public bool IsCancelled { get; }
    public string? ErrorMessage { get; }

    private InstallResult(bool success, bool cancelled, string? errorMsg)
    {
        IsSuccess = success;
        IsCancelled = cancelled;
        ErrorMessage = errorMsg;
    }

    public static InstallResult Success() => new(success: true, cancelled: false, errorMsg: null);
    public static InstallResult Cancelled() => new(success: false, cancelled: true, errorMsg: null);
    public static InstallResult Errored(string? message = null) => new(success: false, cancelled: false, errorMsg: message);

    /// <summary>
    /// <inheritdoc cref="InstallResult{T}.Failed"/>
    /// </summary>
    public static InstallResult Failed<T>(InstallResult<T> innerFailure) where T : notnull
    {
        if (innerFailure.IsSuccess)
            throw new ArgumentException("Cannot fail on a successful result.");

        return new(success: false, cancelled: innerFailure.IsCancelled, errorMsg: innerFailure.ErrorMessage);
    }

    public static InstallResult<T> Success<T>(T value) where T : notnull => InstallResult<T>.Success(value);
    public static InstallResult<T> Cancelled<T>() where T : notnull => InstallResult<T>.Cancelled();
    public static InstallResult<T> Errored<T>(string? message = null) where T : notnull => InstallResult<T>.Errored(message);

    /// <summary>
    /// <inheritdoc cref="InstallResult{T}.Failed"/>
    /// </summary>
    public static InstallResult<T> Failed<T, TInner>(InstallResult<TInner> innerFailure) where T : notnull where TInner : notnull
    {
        return InstallResult<T>.Failed(innerFailure);
    }
}

public readonly struct InstallResult<T> where T : notnull
{
    [MemberNotNullWhen(true, nameof(Value)), MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool IsSuccess { get; } // defaults to false

    public bool IsCancelled { get; }

    [MaybeNull]
    public T? Value => IsSuccess ? value : throw new InvalidOperationException("Install wasn't successful.");
    private readonly T? value;

    [MaybeNull]
    public string? ErrorMessage { get; init; }

    private InstallResult(bool success, bool cancelled, T? value, string? errorMsg)
    {
        IsSuccess = success;
        IsCancelled = cancelled;
        this.value = value;
        ErrorMessage = errorMsg;
    }

    public static InstallResult<T> Success(T value) => new(success: true, cancelled: false, value: value, errorMsg: null);
    public static InstallResult<T> Cancelled() => new(success: false, cancelled: true, value: default, errorMsg: null);
    public static InstallResult<T> Errored(string? message = null) => new(success: false, cancelled: false, value: default, errorMsg: message);

    /// <summary>
    /// Install either failed or was cancelled by the user.
    /// </summary>
    public static InstallResult<T> Failed<TInner>(InstallResult<TInner> innerFailure) where TInner : notnull
    {
        if (innerFailure.IsSuccess)
            throw new ArgumentException("Cannot fail on a successful result.");

        return new(success: false, cancelled: innerFailure.IsCancelled, value: default, errorMsg: innerFailure.ErrorMessage);
    }
}
