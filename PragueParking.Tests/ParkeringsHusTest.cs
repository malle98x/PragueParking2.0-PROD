// ParkeringsHusTest.cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PragueParking.Models;

namespace PragueParking.Test
{
    [TestClass]
    public class ParkeringsHusTests
    {
        [TestMethod]
        public void ParkeraFordon_SkaReturneraTrue_NarDetFinnsPlats()
        {
            var parkeringshus = new ParkeringsHus(antalPlatser: 10, platsStorlek: 4);
            var bil = new Bil("ABC123");

            bool resultat = parkeringshus.ParkeraFordon(bil);

            Assert.IsTrue(resultat);
        }

        [TestMethod]
        public void TaBortFordon_SkaReturneraTrue_NarFordonFinns()
        {
            var parkeringshus = new ParkeringsHus(antalPlatser: 10, platsStorlek: 4);
            var bil = new Bil("JFW556");
            parkeringshus.ParkeraFordon(bil);

            bool resultat = parkeringshus.TaBortFordon("JFW556", out Fordon? borttagetFordon);

            Assert.IsTrue(resultat);
            Assert.IsNotNull(borttagetFordon);
            Assert.AreEqual("JFW556", borttagetFordon.RegistreringsNummer);
        }
    }
}