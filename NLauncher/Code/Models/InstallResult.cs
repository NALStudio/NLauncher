using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Code.Models;

public readonly struct InstallResult
{
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool IsSuccess { get; init; } // defaults to false
    public string? ErrorMessage { get; init; }

    private InstallResult(bool success, string? errorMsg)
    {
        IsSuccess = success;
        ErrorMessage = errorMsg;
    }

    public static InstallResult Success() => new(success: true, errorMsg: null);
    public static InstallResult Cancelled() => new(success: false, errorMsg: null);
    public static InstallResult Errored(string? message = null) => new(success: false, errorMsg: message);

    /// <summary>
    /// <inheritdoc cref="InstallResult{T}.Failed"/>
    /// </summary>
    public static InstallResult Failed<T>(InstallResult<T> innerFailure) where T : notnull
    {
        if (innerFailure.IsSuccess)
            throw new ArgumentException("Cannot fail on a successful result.");

        return new(success: false, innerFailure.ErrorMessage);
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
    public bool IsSuccess { get; init; } // defaults to false

    [MaybeNull]
    public T? Value => IsSuccess ? value : throw new InvalidOperationException("Install wasn't successful.");
    private readonly T? value;

    [MaybeNull]
    public string? ErrorMessage { get; init; }

    private InstallResult(bool success, T? value, string? errorMsg)
    {
        IsSuccess = success;
        this.value = value;
        ErrorMessage = errorMsg;
    }

    public static InstallResult<T> Success(T value) => new(success: true, value: value, errorMsg: null);
    public static InstallResult<T> Cancelled() => new(success: false, value: default, errorMsg: null);
    public static InstallResult<T> Errored(string? message = null) => new(success: false, value: default, errorMsg: message);

    /// <summary>
    /// Install either failed or was cancelled by the user.
    /// </summary>
    public static InstallResult<T> Failed<TInner>(InstallResult<TInner> innerFailure) where TInner : notnull
    {
        if (innerFailure.IsSuccess)
            throw new ArgumentException("Cannot fail on a successful result.");

        return new(success: false, value: default, errorMsg: innerFailure.ErrorMessage);
    }
}
