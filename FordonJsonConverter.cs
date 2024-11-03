// FordonJsonConverter.cs
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PragueParking.Models;

namespace PragueParking
{
    public class FordonJsonConverter : JsonConverter<Fordon>
    {
        public override Fordon Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDocument = JsonDocument.ParseValue(ref reader);
            var jsonObject = jsonDocument.RootElement;

            var typ = jsonObject.GetProperty("Typ").GetString() ?? string.Empty;
            var registreringsNummer = jsonObject.GetProperty("RegistreringsNummer").GetString() ?? string.Empty;
            var ankomstTidStr = jsonObject.GetProperty("AnkomstTid").GetString() ?? string.Empty;

            if (!DateTime.TryParse(ankomstTidStr, out DateTime ankomstTid))
                ankomstTid = DateTime.Now;

            return typ switch
            {
                "Bil" => new Bil(registreringsNummer) { AnkomstTid = ankomstTid },
                "MC" => new MC(registreringsNummer) { AnkomstTid = ankomstTid },
                _ => throw new InvalidOperationException($"Okänd fordonstyp: {typ}")
            };
        }

        public override void Write(Utf8JsonWriter writer, Fordon value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Typ", value.Typ);
            writer.WriteString("RegistreringsNummer", value.RegistreringsNummer);
            writer.WriteNumber("Storlek", value.Storlek);
            writer.WriteString("AnkomstTid", value.AnkomstTid.ToString("o")); // ISO 8601-format
            writer.WriteEndObject();
        }
    }
}