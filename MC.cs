// Models/MC.cs
using System;

namespace PragueParking.Models
{
    public class MC : Fordon
    {
        public MC()
        {
            Storlek = 1;
            Typ = "MC";
        }

        public MC(string registreringsNummer) : base(registreringsNummer, 1)
        {
        }
    }
}