using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonException = Newtonsoft.Json.JsonException;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace GhostOfJoe;

public abstract class SettingBase {
    public string Description { get; set; }
    
    public abstract Type getType();
}

public class Setting<T> : SettingBase {
    public T Value { get; set; }

    public override string ToString() {
        return $"\t- {Description}\n" +
               $"\t- Type: '{typeof(T)}' \n" +
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

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        var settingBase = value as SettingBase;

        writer.WriteStartObject();
        writer.WritePropertyName("Description");
        writer.WriteValue(settingBase.Description);
        writer.WritePropertyName("Type");
        writer.WriteValue(settingBase.getType().AssemblyQualifiedName);
        writer.WritePropertyName("Value");
        serializer.Serialize(writer, ((dynamic)settingBase).Value);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        JObject obj = JObject.Load(reader);

        string description = obj["Description"].ToString();
        string typeName = obj["Type"].ToString();
        JToken valueToken = obj["Value"];

        Type type = Type.GetType(typeName);
        if (type == null)
        {
            throw new JsonException($"Unable to find the type: {typeName}");
        }

        Type settingType = typeof(Setting<>).MakeGenericType(type);
        object value = valueToken.ToObject(type, serializer);

        return Activator.CreateInstance(settingType, description, value);
    }
}

