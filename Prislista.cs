// Models/Prislista.cs
using System;
using System.Collections.Generic;
using System.IO;

namespace PragueParking.Models
{
    public class Prislista
    {
        public Dictionary<string, int> Priser { get; set; } = new Dictionary<string, int>();
        public int GratisTid { get; set; }

        public static Prislista LaddaPrislista(string filNamn)
        {
            if (!File.Exists(filNamn))
                throw new FileNotFoundException($"Prislistan '{filNamn}' hittades inte.");

            var prislista = new Prislista();
            var lines = File.ReadAllLines(filNamn);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                var parts = trimmedLine.Split(':');
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var valuePart = parts[1].Split('#')[0].Trim(); // Ignorera kommentarer

                if (int.TryParse(valuePart, out int value))
                {
                    if (key.Equals("GratisTid", StringComparison.OrdinalIgnoreCase))
                        prislista.GratisTid = value;
                    else
                        prislista.Priser[key] = value;
                }
            }

            return prislista;
        }

        public int BeräknaPris(Fordon fordon, TimeSpan parkeringstid)
        {
            double totalaMinuter = parkeringstid.TotalMinutes - GratisTid;
            if (totalaMinuter <= 0)
                return 0;

            int antalTimmar = (int)Math.Ceiling(totalaMinuter / 60);
            if (Priser.TryGetValue(fordon.Typ, out int prisPerTimme))
                return prisPerTimme * antalTimmar;
            else
                return 0;
        }
    }
}