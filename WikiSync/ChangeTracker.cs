using System.Collections.Concurrent;

namespace WikiSync
{
    public class ChangeTracker
    {
        private readonly ConcurrentDictionary<string, PendingChange> _changes;
        private readonly int _delayMs;

        public ChangeTracker(int delayMs)
        {
            _changes = new ConcurrentDictionary<string, PendingChange>();
            _delayMs = delayMs;
        }

        public bool isDuplicate(string file)
        {
            if (!_changes.TryGetValue(file, out var change))
                return false;

            var now = DateTime.Now;

            return (now - change.LastEvent).TotalMilliseconds < _delayMs;
        }

        public void AddChange(string action, string file)
        {
            _changes[file] = new PendingChange(
                Action: action,
                LastChange: DateTime.Now,
                LastEvent: DateTime.Now
            );
        }

        public IEnumerable<PendingChange> GetAll()
        {
            return _changes.Values;
        }
    }
}
