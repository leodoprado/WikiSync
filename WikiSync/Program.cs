using WikiSync;

// ===================== EDITE AQUI =====================
const string pathWiki = @"C:\Users\leona\OneDrive\Documentos\Repositorio\wiki";
const int delayMs = 200;
// =====================================================

if (!Directory.Exists(pathWiki))
{
    Console.WriteLine($"Pasta não encontrada: {pathWiki}");
    return;
}

var logger = new ConsoleLogger();
var tracker = new ChangeTracker(delayMs);

using var watcher = new FileSystemWatcher(pathWiki)
{
    IncludeSubdirectories = true,
    Filter = "*.md",
    NotifyFilter = NotifyFilters.FileName
                 | NotifyFilters.LastWrite
                 | NotifyFilters.DirectoryName
};

watcher.Created += (_, e) =>
{
    if (!tracker.isDuplicate(e.FullPath))
    {
        tracker.AddChange("CRIADO", e.FullPath);
        logger.Log("CRIADO", e.FullPath);
    }
};

watcher.Changed += (_, e) =>
{
    if (!tracker.isDuplicate(e.FullPath))
    {
        tracker.AddChange("MODIFICADO", e.FullPath);
        logger.Log("MODIFICADO", e.FullPath);
    }
};

watcher.Deleted += (_, e) =>
{
    if (!tracker.isDuplicate(e.FullPath))
    {
        tracker.AddChange("DELETADO", e.FullPath);
        logger.Log("DELETADO", e.FullPath);
    }
};

watcher.Renamed += (_, e) =>
{
    if (!tracker.isDuplicate(e.FullPath))
    {
        tracker.AddChange("RENOMEADO", e.FullPath);
        logger.Log("RENOMEADO", e.FullPath);
    }
};

watcher.Error += (_, e) =>
{
    Console.WriteLine($"[erro] {e.GetException().Message}");
};

watcher.EnableRaisingEvents = true;

Console.WriteLine($"Monitorando: {pathWiki}");
Console.WriteLine("Pressione ENTER para listar pendências.");
Console.WriteLine();

while (true)
{
    Console.ReadLine();

    Console.WriteLine("\n=== PENDÊNCIAS ===");

    foreach (var item in tracker.GetAll())
    {
        Console.WriteLine(
            $"{item.Action} | {item.LastChange:HH:mm:ss}");
    }

    Console.WriteLine();
}

public record PendingChange(
    string Action,
    DateTime LastChange,
    DateTime LastEvent
);