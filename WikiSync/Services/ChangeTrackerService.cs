using WikiSync.Interfaces;

namespace WikiSync.Services
{
    public class ChangeTrackerService : IChangeTrackerService
    {
        private readonly IHashService _hashService;

        private readonly Dictionary<string, string> _knowHashes = new();

        public ChangeTrackerService(IHashService hashService)
        {
            _hashService = hashService;
        }

        public bool HasChanged(string filePath)
        {
            var currentHash = _hashService.GenerateHash(filePath);

            if (!_knowHashes.TryGetValue(filePath, out var oldHash))
            {
                _knowHashes[filePath] = currentHash;
                return true;
            }

            if (oldHash == currentHash)
                return false;

            _knowHashes[filePath] = currentHash;
            return true;
        }
    }
}
