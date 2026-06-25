namespace WikiSync
{
    public class ConsoleLogger
    {
        public void Log(string action, string file)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {action,-11} {file}");
        }
    }
}
