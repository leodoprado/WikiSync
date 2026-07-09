namespace WikiSync.Services
{
    public class FileWatcherService
    {
        private readonly FileSystemWatcher _watcher;
        private readonly Dictionary<string, Timer> _debounceTimers = new();
        public event Action<string>? FileChanged;

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
            Debounce(e.FullPath);
        }

        private void OnFileRenamed(object sender, FileSystemEventArgs e)
        {
            Debounce(e.FullPath);
        }

        private void Debounce(string filePath)
        {
            if (_debounceTimers.TryGetValue(filePath, out var existingTimer))
            {
                existingTimer.Dispose();
                _debounceTimers.Remove(filePath);
            }

            var timer = new Timer(_ =>
            {
                _debounceTimers.Remove(filePath);

                FileChanged?.Invoke(filePath);
            }, null, 500, Timeout.Infinite);

            _debounceTimers[filePath] = timer;
        }
    }
}
