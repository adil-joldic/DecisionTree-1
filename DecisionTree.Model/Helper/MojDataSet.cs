using DecisionTree.Model.Helper;
using DecisionTree.Model.Model;
using static AtributMeta;
using static AtributMeta.KategorickiInfo;

public enum TipAtributa
{
    Kategoricki,
    Numericki
}

public class VrijednostAtributa
{
    public string? Tekst { get; set; }
    public double? Broj { get; set; }

    public bool JeNumericki => TipAtributa == TipAtributa.Numericki;

    public TipAtributa TipAtributa { get; set; }

    public static VrijednostAtributa NapraviKategorijski(string? input)
    {
        return new VrijednostAtributa
        {
            Broj = null,
            Tekst = input,
            TipAtributa = TipAtributa.Numericki
        };
    }

    public static VrijednostAtributa NapraviNumericki(double? input)
    {
        return new VrijednostAtributa
        {
            Broj = input,
            Tekst = null,
            TipAtributa = TipAtributa.Numericki
        };
    }

    public override string ToString() => JeNumericki ? Broj?.ToString("0.###") ?? "" : Tekst ?? "";
}

public class AtributMeta
{
    public class NumerickiInfo
    {
        public required double? Min { get; set; }
        public required double? Max { get; set; }
        public required double? SrednjaVrijednost { get; set; }
        public required double? StandardnaDevijacija { get; set; }
    }

    public class KategorickiInfo
    {
        public class VrijednostBrojac
        {
            public required string Vrijednost { get; set; }
            public required int BrojPojavljivanja { get; set; }
        }

        public required int BrojRazlicitihVrijednosti { get; set; }
        public required List<VrijednostBrojac> Top5Najcescih { get; set; }
    }

    public required string Naziv { get; init; }
    public required TipAtributa TipAtributa { get; init; }

    /// <summary>
    /// Označava da li se ovaj atribut koristi za učenje modela i predikciju.
    /// </summary>
    public bool KoristiZaModel { get; set; } = true;

    public NumerickiInfo? Numericki { get; set; }
    public KategorickiInfo? Kategoricki { get; set; }
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
    public List<string> Historija { get; set; } = [];
    public List<RedPodatka> Podaci { get; set; } = new();
    public List<AtributMeta> Atributi { get; set; } = new();
    public string CiljnaKolona { get; set; }

    public MojDataSet(IEnumerable<string> historija, List<RedPodatka> podaci, List<AtributMeta> atributi, string ciljnaKolona)
    {
        if (podaci.Count == 0)
            throw new ArgumentException("Podaci ne mogu biti prazni.");
        Podaci = podaci;
        this.Historija = historija.ToList();
        CiljnaKolona = ciljnaKolona;
        Atributi = atributi;
        IskljuciAtribute("ID"); // Isključujemo ID ako postoji
        Analiziraj();
    }

    public MojDataSet DodajHistorijskiZapis(string historijskiZapis)
    {
        return new MojDataSet([.. Historija, historijskiZapis], Podaci, Atributi, CiljnaKolona);
    }

    public void IskljuciAtribute(params string[] nazivi)
    {
        foreach (var meta in Atributi)
        {
            if (nazivi.Contains(meta.Naziv))
            {
                if (meta.KoristiZaModel)
                {
                    meta.KoristiZaModel = false;
                    DodajHistorijskiZapis($"Isključeni atribut: {meta.Naziv} iz modela učenja");
                }
            }
        }
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
            new MojDataSet(Historija, trening, Atributi, CiljnaKolona).DodajHistorijskiZapis($"trening {(1 - testProcenat):P0}"),
            new MojDataSet(Historija, test, Atributi, CiljnaKolona).DodajHistorijskiZapis($"test {testProcenat:P0}")
        );
    }

    // 📊 Funkcija za izračun tačnosti predikcije
    public EvaluacijaRezultat Evaluiraj(IKlasifikator klasifikator, MojDataSet testSkup)
    {
        var rezultat = new EvaluacijaRezultat
        {
            Klasifikator = klasifikator.Naziv,
            Parametri = klasifikator.Parametri,
            Historija = [..testSkup.Historija],
            AtributiMeta = [.. testSkup.Atributi],
            VrijemeTreniranjaSek = klasifikator.VrijemeTreniranjaSek
        };

        int tacni = 0;
        int ukupno = testSkup.Podaci.Count;

        var stopwatchEvaluacija = System.Diagnostics.Stopwatch.StartNew();

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
        stopwatchEvaluacija.Stop();

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
        rezultat.VrijemeEvaluacijeSek = stopwatchEvaluacija.ElapsedMilliseconds / 1000.0; // u sekundama

        return rezultat;
    }

    public void DodajKolonuKategorijski(string nazivKolone, Func<RedPodatka, string?> funkcija)
    {
        // Dodaj kolonu u metapodatke ako još ne postoji
        if (!Atributi.Any(a => a.Naziv == nazivKolone))
            Atributi.Add(new AtributMeta { Naziv = nazivKolone, TipAtributa = TipAtributa.Kategoricki });

        // Dodaj vrijednosti u svaki red
        foreach (var red in Podaci)
        {
            string? vrijednost = funkcija(red);
            red.Atributi[nazivKolone] = VrijednostAtributa.NapraviKategorijski(vrijednost);
        }
    }
    public void DodajKolonuNumericki(string nazivKolone, Func<RedPodatka, double?> funkcija)
    {
        // Dodaj kolonu u metapodatke ako još ne postoji
        if (!Atributi.Any(a => a.Naziv == nazivKolone))
            Atributi.Add(new AtributMeta { Naziv = nazivKolone, TipAtributa = TipAtributa.Kategoricki });

        // Dodaj vrijednosti u svaki red
        foreach (var red in Podaci)
        {
            double? vrijednost = funkcija(red);
            red.Atributi[nazivKolone] = VrijednostAtributa.NapraviNumericki(vrijednost);
        }
    }

    public void Analiziraj()
    {
        foreach (var meta in Atributi)
        {
            var vrijednosti = Podaci
                .Select(p => p.Atributi.TryGetValue(meta.Naziv, out var val) ? val : null)
                .Where(v => v != null)
                .ToList();

            if (meta.TipAtributa == TipAtributa.Kategoricki)
            {
                var tekstualne = vrijednosti
                    .Select(v => v!.Tekst)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                meta.Kategoricki = new KategorickiInfo
                {
                    BrojRazlicitihVrijednosti = tekstualne.Distinct().Count(),
                    Top5Najcescih = tekstualne
                        .GroupBy(t => t)
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .Select(g => new VrijednostBrojac{ Vrijednost = g.Key!, BrojPojavljivanja = g.Count() })
                        .ToList()
                };
            }
            else if (meta.TipAtributa == TipAtributa.Numericki)
            {
                var brojevi = vrijednosti
                    .Select(v => v!.Broj)
                    .Where(b => b.HasValue)
                    .Select(b => b!.Value)
                    .ToList();

                if (brojevi.Count > 0)
                {
                    double prosjek = brojevi.Average();
                    double std = Math.Sqrt(brojevi.Average(b => Math.Pow(b - prosjek, 2)));


                    meta.Numericki = new NumerickiInfo
                    {
                        Min = brojevi.Min(),
                        Max = brojevi.Max(),
                        SrednjaVrijednost = prosjek,
                        StandardnaDevijacija = std
                    };
                }
            }
        }
    }

}