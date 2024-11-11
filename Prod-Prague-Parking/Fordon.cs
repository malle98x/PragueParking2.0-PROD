// Models/Fordon.cs
using System;

namespace PragueParking.Models
{
    public abstract class Fordon
    {
        public string RegistreringsNummer { get; set; } = string.Empty;
        public int Storlek { get; set; }
        public string Typ { get; set; } = string.Empty;
        public DateTime AnkomstTid { get; set; }

        protected Fordon()
        {
            AnkomstTid = DateTime.Now;
        }

        protected Fordon(string registreringsNummer, int storlek)
        {
            RegistreringsNummer = registreringsNummer ?? throw new ArgumentNullException(nameof(registreringsNummer));
            Storlek = storlek;
            Typ = GetType().Name;
            AnkomstTid = DateTime.Now;
        }
    }
}