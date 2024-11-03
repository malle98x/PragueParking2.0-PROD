// Models/ParkeringsPlats.cs
using System.Collections.Generic;
using System.Linq;

namespace PragueParking.Models
{
    public class ParkeringsPlats
    {
        public int PlatsNummer { get; set; }
        public int MaxKapacitet { get; set; }
        public List<Fordon> FordonPåPlatsen { get; set; }

        public ParkeringsPlats(int platsNummer, int maxKapacitet)
        {
            PlatsNummer = platsNummer;
            MaxKapacitet = maxKapacitet;
            FordonPåPlatsen = new List<Fordon>();
        }

        public int LedigtUtrymme()
        {
            int upptagenKapacitet = FordonPåPlatsen.Sum(fordon => fordon.Storlek);
            return MaxKapacitet - upptagenKapacitet;
        }

        public bool KanParkera(Fordon fordon)
        {
            if (fordon is Bil)
            {
                return FordonPåPlatsen.Count == 0 && LedigtUtrymme() >= fordon.Storlek;
            }
            else if (fordon is MC)
            {
                if (FordonPåPlatsen.Any(f => f is Bil))
                    return false;

                int antalMC = FordonPåPlatsen.Count(f => f is MC);
                return antalMC < 2 && LedigtUtrymme() >= fordon.Storlek;
            }
            return false;
        }

        public void ParkeraFordon(Fordon fordon)
        {
            FordonPåPlatsen.Add(fordon);
        }

        public void TaBortFordon(Fordon fordon)
        {
            FordonPåPlatsen.Remove(fordon);
        }

        public bool InnehållerFordon(string regNummer)
        {
            return FordonPåPlatsen.Any(f =>
                f.RegistreringsNummer.Equals(regNummer, System.StringComparison.OrdinalIgnoreCase));
        }

        public Fordon? HämtaFordon(string regNummer)
        {
            return FordonPåPlatsen.FirstOrDefault(f =>
                f.RegistreringsNummer.Equals(regNummer, System.StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            if (FordonPåPlatsen.Count == 0)
                return "Tom";

            var fordonInfo = string.Join(" ", FordonPåPlatsen.Select(f =>
                $"{f.Typ} #{f.RegistreringsNummer}"));
            return fordonInfo;
        }
    }
}