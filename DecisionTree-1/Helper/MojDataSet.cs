public class MojDataSet
{
    public List<RedPodatka> Podaci { get; set; } = new();
    public List<string> Atributi { get; set; } = new();

    public MojDataSet(List<RedPodatka> podaci)
    {
        Podaci = podaci;
        if (podaci.Any())
            Atributi = podaci[0].Atributi.Keys.ToList();
    }

    // ✂️ Funkcija za nasumično dijeljenje na trening i test skup
    public (List<RedPodatka> Train, List<RedPodatka> Test) Podijeli(double testProcenat = 0.2)
    {
        var rnd = new Random();
        var izmijesano = Podaci.OrderBy(x => rnd.Next()).ToList();
        int granica = (int)(Podaci.Count * (1 - testProcenat));

        var trening = izmijesano.Take(granica).ToList();
        var test = izmijesano.Skip(granica).ToList();
        return (trening, test);
    }

    // 📊 Funkcija za izračun tačnosti predikcije
    public double IzracunajTacnost(CvorStabla cvor, List<RedPodatka> testSkup)
    {
        int tacni = 0;
        foreach (var red in testSkup)
        {
            var stablo = new StabloOdlučivanja();
            var predikcija = stablo.Predikcija(cvor, red.Atributi);
            if (predikcija == red.Klasa)
                tacni++;
        }

        return (double)tacni / testSkup.Count;
    }

    // 🧮 Dodatno: brojač F1-score po klasama (ako želiš detaljnu evaluaciju)
}
