using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Models;
public readonly record struct OperationResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public string? WarningMessage { get; }

    private OperationResult(bool success, string? error, string? warning)
    {
        IsSuccess = success;
        ErrorMessage = error;
        WarningMessage = warning;
    }

    public static OperationResult Success() => new(success: true, null, null);
    public static OperationResult Warning(string message) => new(success: false, error: null, warning: message);
    public static OperationResult Error(string message) => new(success: false, error: message, warning: null);
}