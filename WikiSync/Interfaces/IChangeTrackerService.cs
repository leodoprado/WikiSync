namespace WikiSync.Interfaces
{
    public interface IChangeTrackerService
    {
        bool HasChanged(string filePath);
    }
}
