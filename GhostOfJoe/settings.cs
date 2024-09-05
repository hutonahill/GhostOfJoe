using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonException = Newtonsoft.Json.JsonException;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace GhostOfJoe;

public class Config {

    public Dictionary<string, SettingBase?> GlobalSettings { get; set; }

    public ulong JoeUserId { get; set; }
        
    public List<ulong> AdminServers { get; set; }
    
    // a logging channel on my personal discord
    public ulong LoggingChannel { get; set; }

    // my user_id
    public ulong AdminUser { get; set; }
    
    public string DISCORD_KEY { get; set; }

    public string pasteBinUsername { get; set; }
    
    public string pasteBinPassword { get; set; }

    public string pasteApiKey { get; set; }

    public string flowPasteKey { get; set; }

    public Config() {
        GlobalSettings = new Dictionary<string, SettingBase?> {
            { "NsfwFlow", new Setting<bool>("Flow is considered NSFW", true) },
            { "ScoreRoundsTo", new Setting<uint>("Round scores to this place", 10) },
            {"BlacklistedUsers", new Setting<List<ulong>>("User Ids that may not interact with the bot", new List<ulong>())}
        };

        JoeUserId = 238096751607676928;
        //                              my personal server,  The party bus
        AdminServers = new List<ulong> { 859184385889796096, 573289805472071680 };
        
        LoggingChannel = 1269384832064950384;
        
        AdminUser = 168496575369183232;
        
        DISCORD_KEY = "DISCORD API KEY";

        pasteBinPassword = "PASSWORD";
        pasteBinUsername = "USERNAME";

        pasteApiKey = "PASTE API KEY";

        flowPasteKey = "taDVgTGF";
    }
}
    


public abstract class SettingBase {
    public string? Description { get; init; }
    
    public abstract Type getType();
}

public class Setting<T> : SettingBase {
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
        return typeof(SettingBase).IsAssignableFrom(objectType);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        var settingBase = value as SettingBase;

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

