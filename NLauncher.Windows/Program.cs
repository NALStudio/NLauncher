using Microsoft.Extensions.DependencyInjection;
using NLauncher.Services;
using NLauncher.Services.Index;
using NLauncher.Services.Settings;
using NLauncher.Shared.AppHandlers;
using NLauncher.Shared.AppHandlers.Base;
using NLauncher.Web.Services;
using System.Net.Http.Headers;

namespace NLauncher.Windows;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainPage());
    }
}