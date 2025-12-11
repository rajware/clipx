using System.CommandLine;
using ClipX.Core.Interfaces;
using ClipX.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClipX.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Set up dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Create root command
        var rootCommand = new RootCommand("ClipX - Cross-platform clipboard manager");

        // Add copy command
        var copyCommand = new Command("copy", "Copy text from stdin to clipboard");
        copyCommand.SetHandler(async () =>
        {
            var clipboardManager = serviceProvider.GetRequiredService<ClipboardManager>();
            var success = await clipboardManager.CopyFromStdinAsync();
            Environment.Exit(success ? 0 : 1);
        });
        rootCommand.AddCommand(copyCommand);

        // Add paste command
        var pasteCommand = new Command("paste", "Paste text from clipboard to stdout");
        pasteCommand.SetHandler(async () =>
        {
            var clipboardManager = serviceProvider.GetRequiredService<ClipboardManager>();
            var success = await clipboardManager.PasteToStdoutAsync();
            Environment.Exit(success ? 0 : 1);
        });
        rootCommand.AddCommand(pasteCommand);

        // Parse and execute
        return await rootCommand.InvokeAsync(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register platform-specific providers
        services.AddSingleton<IClipboardProvider>(Platform.PlatformFactory.CreateClipboardProvider());
        services.AddSingleton<IFileSystemProvider>(Platform.PlatformFactory.CreateFileSystemProvider());

        // Register core services
        services.AddSingleton<HistoryManager>();
        services.AddSingleton<ClipboardManager>(sp =>
        {
            var clipboardProvider = sp.GetRequiredService<IClipboardProvider>();
            var historyManager = sp.GetRequiredService<HistoryManager>();
            var platformId = Platform.PlatformFactory.GetPlatformIdentifier();
            return new ClipboardManager(clipboardProvider, historyManager, platformId);
        });
    }
}
