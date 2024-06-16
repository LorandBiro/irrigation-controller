﻿using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;
using System.Text.Json;

namespace IrrigationController.Adapters
{
    public class ValveRepository : IValveRepository
    {
        private readonly Dictionary<int, Valve> valves = [];
        private readonly string path;

        public ValveRepository(string appDataPath)
        {
            this.path = Path.Join(appDataPath, "valves.json");
            if (!File.Exists(this.path))
            {
                return;
            }

            List<Valve>? valves = JsonSerializer.Deserialize<List<Valve>>(File.ReadAllText(this.path));
            if (valves is null)
            {
                return;
            }

            this.valves = valves.ToDictionary(x => x.Id, x => x);
        }

        public Valve? Get(int id)
        {
            this.valves.TryGetValue(id, out Valve? valve);
            return valve;
        }

        public void Save(Valve valve)
        {
            this.valves[valve.Id] = valve;
            File.WriteAllText(this.path, JsonSerializer.Serialize(this.valves.Values));
        }
    }
}
