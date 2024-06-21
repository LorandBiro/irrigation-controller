using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;
using IrrigationController.Core.Domain;

namespace IrrigationController.Adapters;

public class IrrigationLog : IIrrigationLog
{
    private static readonly JsonSerializerOptions Options = new() { TypeInfoResolver = new PolymorphicTypeResolver(), PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }};

    private readonly ILogger<IrrigationLog> logger;
    private readonly string path;

    private readonly List<IIrrigationEvent> events = [];

    public IrrigationLog(ILogger<IrrigationLog> logger, Config config)
    {
        this.logger = logger;
        this.path = Path.Combine(config.AppDataPath, "irrigation-log.json");
        if (!File.Exists(this.path))
        {
            return;
        }

        this.events = File.ReadAllLines(this.path).Select(x => JsonSerializer.Deserialize<IIrrigationEvent>(x, Options)!).ToList();
        this.logger.LogDebug("Loaded {Count} events", this.events.Count);
    }

    public event EventHandler? LogUpdated;

    public IReadOnlyList<IIrrigationEvent> Get(int limit)
    {
        limit = Math.Min(limit, this.events.Count);
        return this.events[^limit..];
    }

    public IReadOnlyList<IIrrigationEvent> GetAll()
    {
        return this.events;
    }

    public void Write(IIrrigationEvent e)
    {
        string line = JsonSerializer.Serialize(e, Options);
        this.logger.LogDebug("{Event}", line);
        File.AppendAllLines(this.path, [line]);
        this.events.Add(e);
        this.LogUpdated?.Invoke(this, EventArgs.Empty);
    }

    public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);
            if (jsonTypeInfo.Type == typeof(IIrrigationEvent))
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "type",
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                    DerivedTypes =
                    {
                        new JsonDerivedType(typeof(ZoneOpened), "zoneOpened"),
                        new JsonDerivedType(typeof(ZoneClosed), "zoneClosed"),
                        new JsonDerivedType(typeof(RainDetected), "rainDetected"),
                        new JsonDerivedType(typeof(RainCleared), "rainCleared"),
                        new JsonDerivedType(typeof(ShortCircuitDetected), "shortCircuitDetected"),
                        new JsonDerivedType(typeof(ShortCircuitResolved), "shortCircuitResolved"),
                    }
                };
            }

            return jsonTypeInfo;
        }
    }
}
