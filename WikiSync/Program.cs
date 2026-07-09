using WikiSync.Interfaces;
using WikiSync.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IHashService, HashService>();
builder.Services.AddSingleton<IChangeTrackerService, ChangeTrackerService>();
builder.Services.AddSingleton<FileWatcherService>();

var app = builder.Build();

var watcher = app.Services.GetRequiredService<FileWatcherService>();
var tracker = app.Services.GetRequiredService<IChangeTrackerService>();

watcher.FileChanged += filePath =>
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"Arquivo removido: {filePath}");
        return;
    }

    var changed = tracker.HasChanged(filePath);

    if (changed)
        Console.WriteLine($"Arquivo alterado e precisa sincronizar: {filePath}");
    else
        Console.WriteLine($"Arquivo ignorado, conteúdo não mudou: {filePath}");
};

watcher.Start(@"C:\Users\leona\OneDrive\Documentos\Repositorio\wiki");

app.MapControllers();

app.Run();