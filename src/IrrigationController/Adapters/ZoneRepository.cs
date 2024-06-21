using IrrigationController.Core.Domain;
using System.Text.Json;

namespace IrrigationController.Adapters;

public class ZoneRepository : IZoneRepository
{
    private readonly Dictionary<int, Zone> zones = [];
    private readonly string path;

    public ZoneRepository(Config config)
    {
        this.path = Path.Join(config.AppDataPath, "zones.json");
        if (!File.Exists(this.path))
        {
            return;
        }

        List<Zone>? zones = JsonSerializer.Deserialize<List<Zone>>(File.ReadAllText(this.path));
        if (zones is null)
        {
            return;
        }

        this.zones = zones.ToDictionary(x => x.Id, x => x);
    }

    public event EventHandler? Changed;

    public Zone? Get(int id)
    {
        this.zones.TryGetValue(id, out Zone? zone);
        return zone;
    }

    public IReadOnlyList<Zone> GetAll()
    {
        return this.zones.Values.ToList();
    }

    public void Save(Zone zone)
    {
        this.zones[zone.Id] = zone;
        File.WriteAllText(this.path, JsonSerializer.Serialize(this.zones.Values));
        this.Changed?.Invoke(this, EventArgs.Empty);
    }
}
