using WikiSync.Models;

namespace WikiSync.Services
{
    public class FileWatcherService
    {
        private readonly FileSystemWatcher _watcher;
        private readonly Dictionary<string, Timer> _debounceTimers = new();
        public event Action<FileChange>? FileChanged;

        public FileWatcherService()
        {
            _watcher = new FileSystemWatcher();
        }

        public void Start(string folderPath)
        {
            _watcher.Path = folderPath;
            _watcher.Filter = "*.md";
            _watcher.IncludeSubdirectories = true;

            _watcher.NotifyFilter = NotifyFilters.FileName |
                                    NotifyFilters.LastWrite |
                                    NotifyFilters.CreationTime;
            
            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Renamed += OnFileChanged;
            _watcher.Deleted += OnFileChanged;
        
            _watcher.EnableRaisingEvents = true;

            Console.WriteLine($"Monitoring folder: {folderPath}");
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            var changeType = e.ChangeType switch
            {
                WatcherChangeTypes.Created => FileChangeType.Created,
                WatcherChangeTypes.Deleted => FileChangeType.Deleted,
                _ => FileChangeType.Updated
            };

            Debounce(new FileChange
            {
                ChangeType = changeType,
                FilePath = e.FullPath
            });
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            Debounce(new FileChange
            {
                ChangeType = FileChangeType.Renamed,
                FilePath = e.FullPath,
                OldFilePath = e.OldFullPath
            });
        }

        private void Debounce(FileChange fileChange)
        {
            var key = fileChange.ChangeType == FileChangeType.Renamed
                ? $"{fileChange.OldFilePath}|{fileChange.FilePath}"
                : fileChange.FilePath;

            if (_debounceTimers.TryGetValue(key, out var existingTimer))
            {
                existingTimer.Dispose();
                _debounceTimers.Remove(key);
            }

            var timer = new Timer(_ =>
            {
                _debounceTimers.Remove(key);
                FileChanged?.Invoke(fileChange);
            }, null, 500, Timeout.Infinite);

            _debounceTimers[key] = timer;
        }
    }
}
