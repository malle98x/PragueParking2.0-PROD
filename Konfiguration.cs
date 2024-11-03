// Models/Konfiguration.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PragueParking.Models
{
    public class Konfiguration
    {
        public int AntalPlatser { get; set; }
        public int PlatsStorlek { get; set; }
        public List<Fordonstyp> Fordonstyper { get; set; } = new List<Fordonstyp>();

        public static Konfiguration LäsInKonfiguration(string filSökväg)
        {
            if (File.Exists(filSökväg))
            {
                string jsonData = File.ReadAllText(filSökväg);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var konfiguration = JsonSerializer.Deserialize<Konfiguration>(jsonData, options);
                return konfiguration ?? new Konfiguration();
            }
            else
            {
                throw new FileNotFoundException($"Konfigurationsfilen '{filSökväg}' hittades inte.");
            }
        }
    }

    public class Fordonstyp
    {
        public string Typ { get; set; } = string.Empty;
        public int Storlek { get; set; }
        public int MaxPerPlats { get; set; }
    }
}