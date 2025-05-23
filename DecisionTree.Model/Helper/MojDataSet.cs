using DecisionTree.Model.Helper;
using DecisionTree.Model.Model;

public enum TipAtributa
{
    Kategoricki,
    Numericki
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
    public Dictionary<string, string> Atributi { get; set; } = new();

    /// <summary>
    /// Oznaka ciljne klase (npr. "Igraj", "Ne igraj")
    /// </summary>
    public string Klasa { get; set; } = string.Empty;
}


public class MojDataSet
{
    public List<RedPodatka> Podaci { get; set; } = new();
    public List<AtributMeta> Atributi { get; set; } = new();
    public string CiljnaKolona { get; set; }

    public MojDataSet(List<RedPodatka> podaci, string ciljnaKolona)
    {
        if (podaci == null || !podaci.Any())
            throw new ArgumentException("Podaci ne mogu biti prazni.");


        Podaci = podaci;
        CiljnaKolona = ciljnaKolona;

        Atributi = podaci[0].Atributi
            .Select(kvp => new AtributMeta
            {
                Naziv = kvp.Key,
                TipAtributa = podaci
                        .Where(p => !string.IsNullOrWhiteSpace(p.Atributi[kvp.Key]))
                        .All(p => double.TryParse(p.Atributi[kvp.Key], out _))
                            ? TipAtributa.Numericki
                            : TipAtributa.Kategoricki
            }).ToList();
    }

    // ✂️ Funkcija za nasumično dijeljenje na trening i test skup
    public (MojDataSet Train, MojDataSet Test) Podijeli(double testProcenat = 0.2, int? random_state = null)
    {
        // todo: za isti random_stati treba dobiti isti rezultat
        // ako je random_state null, koristi se trenutni sistemski random
        //todo: implementirati podjelu sa random_state, da bude konzistentno
        var random = new Random(random_state ?? DateTime.Now.Millisecond);

        var rnd = new Random();
        var izmijesano = Podaci.OrderBy(x => rnd.Next()).ToList();
        int granica = (int)(Podaci.Count * (1 - testProcenat));

        var trening = izmijesano.Take(granica).ToList();
        var test = izmijesano.Skip(granica).ToList();

        return (
            new MojDataSet(trening, CiljnaKolona),
            new MojDataSet(test, CiljnaKolona)
        );
    }

    // 📊 Funkcija za izračun tačnosti predikcije
    public EvaluacijaRezultat Evaluiraj(IKlasifikator klasifikator, MojDataSet testSkup)
    {
        var rezultat = new EvaluacijaRezultat();

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

        rezultat.Accuracy = tacni / (double)ukupno;
        rezultat.UkupnoTestiranih = ukupno;
        rezultat.UspjesnoPredvidjeno = tacni;

        return rezultat;
    }

}
