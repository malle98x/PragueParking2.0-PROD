// Models/Bil.cs
namespace PragueParking.Models
{
    public class Bil : Fordon
    {
        public Bil()
        {
            Storlek = 2;
            Typ = "Bil";
        }

        public Bil(string registreringsNummer) : base(registreringsNummer, 2)
        {
        }
    }
}