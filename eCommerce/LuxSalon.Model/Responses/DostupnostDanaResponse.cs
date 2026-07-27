namespace LuxSalon.Model.Responses
{
    /// <summary>
    /// Dostupnost jednog dana u mjesecu za odabranog frizera - koristi se za bojenje
    /// color-coded kalendara u mobilnoj aplikaciji (zeleno = ima slobodnih termina, crveno = nema).
    /// </summary>
    public class DostupnostDanaResponse
    {
        public DateTime Datum { get; set; }

        // Da li frizer uopste radi tog dana (RadnoVrijeme postoji i NeRadi = false)
        public bool Radi { get; set; }

        // Da li ima bar jedan slobodan termin tog dana (samo relevantno ako Radi = true)
        public bool Slobodno { get; set; }

        public int BrojSlobodnihSlotova { get; set; }
    }
}
