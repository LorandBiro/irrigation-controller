using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Adapters
{
    public class IrrigationLog : IIrrigationLog
    {
        private readonly ILogger<IrrigationLog> logger;
        private readonly string path;

        private readonly List<string> lines = [];

        public IrrigationLog(ILogger<IrrigationLog> logger, Config config)
        {
            this.logger = logger;
            this.path = Path.Combine(config.AppDataPath, "irrigation-log.txt");
            if (!File.Exists(this.path))
            {
                return;
            }

            this.lines = [.. File.ReadAllLines(this.path)];
        }

        public event EventHandler? LogUpdated;

        public IReadOnlyList<string> Get(int limit)
        {
            limit = Math.Min(limit, this.lines.Count);
            return this.lines[^limit..];
        }

        public IReadOnlyList<string> GetAll()
        {
            return this.lines;
        }

        public void Write(string message)
        {
            this.logger.LogInformation("{Message}", message);
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            File.AppendAllLines(this.path, [line]);
            this.lines.Add(line);
            this.LogUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
