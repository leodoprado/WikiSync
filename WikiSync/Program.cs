using WikiSync.BackgroundServices;
using WikiSync.Interfaces;
using WikiSync.Models;
using WikiSync.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IHashService, HashService>();
builder.Services.AddSingleton<IChangeTrackerService, ChangeTrackerService>();
builder.Services.AddSingleton<ISyncQueueService, SyncQueueService>();
builder.Services.AddSingleton<ISyncService, SyncService>();
builder.Services.AddSingleton<IStorageService, StorageService>();
builder.Services.AddSingleton<FileWatcherService>();

builder.Services.AddHttpClient<IWikiSyncApiClient, WikiSyncApiClient>(
    (serviceProvider, httpClient) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var apiBaseUrl =
            configuration["WikiSync:ApiBaseUrl"]
            ?? throw new InvalidOperationException(
                "A URL da API não foi configurada.");

        httpClient.BaseAddress = new Uri(
            apiBaseUrl.TrimEnd('/') + "/");
    });

builder.Services.AddHostedService<SyncWorker>();

var app = builder.Build();

var watcher =
    app.Services.GetRequiredService<FileWatcherService>();

var tracker =
    app.Services.GetRequiredService<IChangeTrackerService>();

var syncQueue =
    app.Services.GetRequiredService<ISyncQueueService>();

watcher.FileChanged += fileChange =>
{
    if (fileChange.ChangeType is FileChangeType.Deleted
        or FileChangeType.Renamed)
    {
        syncQueue.Enqueue(fileChange);
        return;
    }

    if (!File.Exists(fileChange.FilePath))
        return;

    try
    {
        var changed = tracker.HasChanged(fileChange.FilePath);

        if (!changed)
            return;

        syncQueue.Enqueue(fileChange);
    }
    catch (IOException exception)
    {
        Console.WriteLine(
            $"Arquivo ainda está em uso: {exception.Message}");
    }
};

watcher.Start(
    @"C:\Users\leona\OneDrive\Documentos\Repositorio\wiki");

app.MapControllers();

app.Run();