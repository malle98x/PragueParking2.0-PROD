// Models/ParkeringsHus.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PragueParking.Models
{
    public class ParkeringsHus
    {
        public List<ParkeringsPlats> ParkeringsPlatser { get; set; }

        public ParkeringsHus(int antalPlatser, int platsStorlek)
        {
            ParkeringsPlatser = new List<ParkeringsPlats>();
            for (int i = 1; i <= antalPlatser; i++)
                ParkeringsPlatser.Add(new ParkeringsPlats(i, platsStorlek));
        }

        public bool ParkeraFordon(Fordon fordon)
        {
            foreach (var plats in ParkeringsPlatser)
            {
                if (plats.KanParkera(fordon))
                {
                    plats.ParkeraFordon(fordon);
                    return true;
                }
            }
            return false;
        }

        public bool FlyttaFordon(string regNummer, int nyPlatsNummer)
        {
            if (FindFordon(regNummer, out ParkeringsPlats? nuvarandePlats, out Fordon? fordon))
            {
                var nyPlats = ParkeringsPlatser[nyPlatsNummer - 1];
                if (nyPlats.KanParkera(fordon))
                {
                    nuvarandePlats.TaBortFordon(fordon);
                    nyPlats.ParkeraFordon(fordon);
                    return true;
                }
            }
            return false;
        }

        public bool TaBortFordon(string regNummer, out Fordon? fordon)
        {
            if (FindFordon(regNummer, out ParkeringsPlats? plats, out fordon))
            {
                plats.TaBortFordon(fordon);
                return true;
            }
            fordon = null;
            return false;
        }

        public bool FindFordon(string regNummer, out ParkeringsPlats? plats)
        {
            foreach (var p in ParkeringsPlatser)
            {
                if (p.InnehållerFordon(regNummer))
                {
                    plats = p;
                    return true;
                }
            }
            plats = null;
            return false;
        }

        public bool FindFordon(string regNummer, out ParkeringsPlats? plats, out Fordon? fordon)
        {
            foreach (var p in ParkeringsPlatser)
            {
                var f = p.HämtaFordon(regNummer);
                if (f != null)
                {
                    plats = p;
                    fordon = f;
                    return true;
                }
            }
            plats = null;
            fordon = null;
            return false;
        }

        public void SparaData(string filNamn)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new FordonJsonConverter() }
            };
            string jsonData = JsonSerializer.Serialize(ParkeringsPlatser, options);
            File.WriteAllText(filNamn, jsonData);
        }

        public void LaddaData(string filNamn)
        {
            if (File.Exists(filNamn))
            {
                string jsonData = File.ReadAllText(filNamn);
                var options = new JsonSerializerOptions
                {
                    Converters = { new FordonJsonConverter() }
                };
                ParkeringsPlatser = JsonSerializer.Deserialize<List<ParkeringsPlats>>(jsonData, options) ?? new List<ParkeringsPlats>();
            }
        }
    }
}