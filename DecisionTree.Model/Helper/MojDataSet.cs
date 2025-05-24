using DecisionTree.Model.Helper;
using DecisionTree.Model.Model;

public enum TipAtributa
{
    Kategoricki,
    Numericki
}

public class VrijednostAtributa
{
    public string? Tekst { get; set; }
    public double? Broj { get; set; }

    public bool JeNumericki => Broj.HasValue;

    public TipAtributa TipAtributa { get; set; }

    public VrijednostAtributa(string input, TipAtributa tipAtributa)
    {
        TipAtributa = tipAtributa;
        Tekst = input;

        if (tipAtributa == TipAtributa.Numericki && double.TryParse(input, out var broj))
            Broj = broj;
        else
            Broj = null;
    }

    public override string ToString() => JeNumericki ? Broj?.ToString("0.###") ?? "" : Tekst ?? "";
}

public class AtributMeta
{
    public string Naziv { get; set; } = string.Empty;
    public TipAtributa TipAtributa { get; set; } = TipAtributa.Kategoricki;
    public bool IsDeleted { get; set; } = false;
}

public class RedPodatka
{
    /// <summary>
    /// Vrijednosti atributa za jedan red (npr. {"Vrijeme": "sunčano", "Temperatura": "toplo"})
    /// </summary>
    public Dictionary<string, VrijednostAtributa> Atributi { get; set; } = new();

    /// <summary>
    /// Oznaka ciljne klase (npr. "Igraj", "Ne igraj")
    /// </summary>
    public string Klasa { get; set; } = string.Empty;
}


public class MojDataSet
{
    public string Naziv { get; set; } = string.Empty;
    public List<RedPodatka> Podaci { get; set; } = new();
    public List<AtributMeta> Atributi { get; set; } = new();
    public string CiljnaKolona { get; set; }

    public MojDataSet(string naziv, List<RedPodatka> podaci, List<AtributMeta> atributi, string ciljnaKolona)
    {
        if (podaci.Count == 0)
            throw new ArgumentException("Podaci ne mogu biti prazni.");
        Podaci = podaci;
        Naziv = naziv;
        CiljnaKolona = ciljnaKolona;
        Atributi = atributi;
    }

    // ✂️ Funkcija za nasumično dijeljenje na trening i test skup
    public (MojDataSet Train, MojDataSet Test) Podijeli(double testProcenat = 0.2, int? random_state = null)
    {
        // ako je random_state null, koristi se trenutni sistemski random
        var random = new Random(random_state ?? DateTime.Now.Millisecond);

        var izmijesano = Podaci.OrderBy(x => random.Next()).ToList();
        int granica = (int)(Podaci.Count * (1 - testProcenat));

        List<RedPodatka> trening = izmijesano.Take(granica).ToList();
        List<RedPodatka> test = izmijesano.Skip(granica).ToList();

        return (
            new MojDataSet(Naziv + "-trening", trening, this.Atributi, this.CiljnaKolona),
            new MojDataSet(Naziv + "-test", test, this.Atributi, this.CiljnaKolona)
        );
    }

    // 📊 Funkcija za izračun tačnosti predikcije
    public EvaluacijaRezultat Evaluiraj(IKlasifikator klasifikator, MojDataSet testSkup)
    {
        var rezultat = new EvaluacijaRezultat
        {
            Klasifikator = klasifikator.Naziv,
            Parametri = klasifikator.Parametri,
        };

        int tacni = 0;
        int ukupno = testSkup.Podaci.Count;

        foreach (var red in testSkup.Podaci)
        {
            string stvarna = red.Klasa;
            string predikcija = klasifikator.Predikcija(red);

            // Broj stvarnih po klasama
            if (!rezultat.SveKlase.ContainsKey(stvarna))
                rezultat.SveKlase[stvarna] = 0;
            rezultat.SveKlase[stvarna]++;

            // Confusion matrix: ključ kao string "stvarna=>predikcija"
            string kljuc = $"{stvarna}=>{predikcija}";
            if (!rezultat.ConfusionMatrix.ContainsKey(kljuc))
                rezultat.ConfusionMatrix[kljuc] = 0;
            rezultat.ConfusionMatrix[kljuc]++;

            if (stvarna == predikcija)
                tacni++;
        }

        foreach (var klasa in rezultat.SveKlase.Keys)
        {
            int tp = rezultat.ConfusionMatrix.TryGetValue($"{klasa}=>{klasa}", out var valTP) ? valTP : 0;

            int fp = rezultat.ConfusionMatrix
                .Where(kvp => kvp.Key.EndsWith($"=>{klasa}") && !kvp.Key.StartsWith($"{klasa}=>"))
                .Sum(kvp => kvp.Value);

            int fn = rezultat.ConfusionMatrix
                .Where(kvp => kvp.Key.StartsWith($"{klasa}=>") && !kvp.Key.EndsWith($"=>{klasa}"))
                .Sum(kvp => kvp.Value);

            double prec = (tp + fp) > 0 ? tp / (double)(tp + fp) : 0;
            double rec = (tp + fn) > 0 ? tp / (double)(tp + fn) : 0;
            double f1 = (prec + rec) > 0 ? 2 * prec * rec / (prec + rec) : 0;

            rezultat.Precision[klasa] = prec;
            rezultat.Recall[klasa] = rec;
            rezultat.F1Score[klasa] = f1;
        }

        rezultat.NazivDataSeta = testSkup.Naziv;
        rezultat.Accuracy = tacni / (double)ukupno;
        rezultat.UkupnoTestiranih = ukupno;
        rezultat.UspjesnoPredvidjeno = tacni;

        return rezultat;
    }

}
