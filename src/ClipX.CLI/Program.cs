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

        // Add history command
        var historyCommand = new Command("history", "View clipboard history");
        var limitOption = new Option<int>(
            name: "--limit",
            description: "Maximum number of entries to show (0 = all)",
            getDefaultValue: () => 10);
        historyCommand.AddOption(limitOption);
        historyCommand.SetHandler(async (int limit) =>
        {
            var historyManager = serviceProvider.GetRequiredService<HistoryManager>();
            var entries = await historyManager.GetHistoryAsync(limit);
            
            if (entries.Count == 0)
            {
                Console.WriteLine("No clipboard history");
                Environment.Exit(0);
            }

            foreach (var entry in entries)
            {
                var shortId = entry.Id.Substring(0, Math.Min(8, entry.Id.Length));
                var timestamp = entry.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                var preview = entry.Content.Length > 60 
                    ? entry.Content.Substring(0, 60) + "..." 
                    : entry.Content;
                
                Console.WriteLine($"ID: {shortId} ({timestamp})");
                Console.WriteLine(preview);
                Console.WriteLine("---");
            }
            
            Environment.Exit(0);
        }, limitOption);
        rootCommand.AddCommand(historyCommand);

        // Add restore command
        var restoreCommand = new Command("restore", "Restore entry from history to clipboard");
        var idArgument = new Argument<string>("id", "Entry ID or position (1 = most recent)");
        restoreCommand.AddArgument(idArgument);
        restoreCommand.SetHandler(async (string id) =>
        {
            var historyManager = serviceProvider.GetRequiredService<HistoryManager>();
            var clipboardProvider = serviceProvider.GetRequiredService<IClipboardProvider>();
            
            // Try to parse as position first
            ClipX.Core.Models.ClipboardEntry? entry = null;
            if (int.TryParse(id, out int position))
            {
                entry = await historyManager.GetEntryByPositionAsync(position);
            }
            else
            {
                entry = await historyManager.GetEntryByIdAsync(id);
            }

            if (entry == null)
            {
                Console.Error.WriteLine($"Error: Entry '{id}' not found in history");
                Environment.Exit(1);
            }

            await clipboardProvider.SetTextAsync(entry.Content);
            Console.WriteLine($"Restored entry to clipboard: {entry.Content.Substring(0, Math.Min(60, entry.Content.Length))}...");
            Environment.Exit(0);
        }, idArgument);
        rootCommand.AddCommand(restoreCommand);

        // Add clear command
        var clearCommand = new Command("clear", "Clear clipboard history");
        var allOption = new Option<bool>("--all", "Clear all history");
        var beforeOption = new Option<DateTime?>("--before", "Clear entries before this date");
        var forceOption = new Option<bool>("--force", "Skip confirmation prompt");
        clearCommand.AddOption(allOption);
        clearCommand.AddOption(beforeOption);
        clearCommand.AddOption(forceOption);
        clearCommand.SetHandler(async (bool all, DateTime? before, bool force) =>
        {
            var historyManager = serviceProvider.GetRequiredService<HistoryManager>();

            if (!all && before == null)
            {
                Console.Error.WriteLine("Error: Must specify --all or --before");
                Environment.Exit(1);
            }

            // Confirmation prompt unless --force
            if (!force)
            {
                var message = all ? "Clear all clipboard history?" : $"Clear entries before {before}?";
                Console.Write($"{message} (y/N): ");
                var response = Console.ReadLine();
                if (response?.ToLower() != "y")
                {
                    Console.WriteLine("Cancelled");
                    Environment.Exit(0);
                }
            }

            if (all)
            {
                await historyManager.ClearHistoryAsync();
                Console.WriteLine("Clipboard history cleared");
            }
            else if (before != null)
            {
                await historyManager.ClearHistoryBeforeAsync(before.Value);
                Console.WriteLine($"Cleared entries before {before}");
            }

            Environment.Exit(0);
        }, allOption, beforeOption, forceOption);
        rootCommand.AddCommand(clearCommand);

        // Parse and execute
        return await rootCommand.InvokeAsync(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register platform-specific providers
        var fileSystemProvider = Platform.PlatformFactory.CreateFileSystemProvider();
        services.AddSingleton<IClipboardProvider>(Platform.PlatformFactory.CreateClipboardProvider());
        services.AddSingleton<IFileSystemProvider>(fileSystemProvider);

        // Register storage and configuration
        services.AddSingleton<StorageManager>();
        services.AddSingleton<ConfigurationManager>();

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
