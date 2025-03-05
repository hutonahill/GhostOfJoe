using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonException = Newtonsoft.Json.JsonException;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace GhostOfJoe;

public class DiscordOptions {
    
    public const string SectionName = "BotSettings";
    
    public Dictionary<string, ISetting?> GlobalSettings { get; set; } = new() {
        { "NsfwFlow", new Setting<bool>("Flow is considered NSFW", true) },
        { "ScoreRoundsTo", new Setting<uint>("Round scores to this place", 10) }
    };

    public ulong JoeUserId { get; set; } = 238096751607676928;

    public List<ulong> AdminServers { get; set; } = new() { 859184385889796096, 573289805472071680 };

    // a logging channel on my personal discord
    public ulong LoggingChannel { get; set; } = 1269384832064950384;

    // my user_id
    public ulong AdminUser { get; set; } = 168496575369183232;

    public string DISCORD_KEY { get; set; } = "DISCORD API KEY";

    public string pasteBinUsername { get; set; } = "USERNAME";

    public string pasteBinPassword { get; set; } = "PASSWORD";

    public string pasteApiKey { get; set; } = "PASTE API KEY";

    public string flowPasteKey { get; set; } = "taDVgTGF";

    public List<ulong> BlacklistedUsers { get; set; } = new();

    //                              my personal server,  The party bus
}
    


public abstract class ISetting {
    public string? Description { get; init; }
    
    public abstract Type getType();
}

public class Setting<T> : ISetting {
    public T Value { get; set; }

    public override string ToString() {
        return $"\t- {Description}\n" +
               $"\t- Type: '{typeof(T).Name}' \n" +
               $"\t- Value: '{Value}'";
    }

    public override Type getType() {
        return typeof(T);
    }
    
    public Setting(string description, T value) {
        Description = description;
        Value = value;
    }
}

public class SettingBaseConverter : JsonConverter {

    public override bool CanConvert(Type objectType) {
        return typeof(ISetting).IsAssignableFrom(objectType);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        var settingBase = value as ISetting;

        Debug.Assert(settingBase != null, nameof(settingBase) + " != null");

        writer.WriteStartObject();
        writer.WritePropertyName("Description");
        writer.WriteValue(settingBase.Description);
        writer.WritePropertyName("Type");
        writer.WriteValue(settingBase.GetType().FullName); // Fixed typo to `GetType`
        writer.WritePropertyName("Value");
        serializer.Serialize(writer, ((dynamic)settingBase).Value); // Use dynamic to handle different value types
        writer.WriteEndObject();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
        JObject obj = JObject.Load(reader);

        string description = obj["Description"]?.ToString() ?? "";
        string typeName = obj["Type"]?.ToString() ?? "";
        JToken? valueToken = obj["Value"];

        // Resolve the type from the type name
        Type? type = Type.GetType(typeName);
        if (type == null) {
            throw new JsonException($"Unable to find the type: {typeName}");
        }

        // Create a generic Setting<T> type based on the resolved type
        Type settingType = typeof(Setting<>).MakeGenericType(type);
        object? value = valueToken?.ToObject(type, serializer); // Deserialize the value to the expected type

        // Return a new instance of Setting<T> with the deserialized description and value
        return Activator.CreateInstance(settingType, description, value);
    }
}

