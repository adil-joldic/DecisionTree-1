using DecisionTree.Model.DataSet;
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
            TipAtributa = TipAtributa.Kategoricki
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
        public required double? Medijana { get; set; }
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
    public string Klasa => Atributi[MojDataSet.CiljnaKolona!]!.Tekst!;

    public MojDataSet MojDataSet { get; set; } = null!; // Postavlja se prilikom dodavanja u MojDataSet

    public string? GetText(string naziv)
    {
        return Atributi.TryGetValue(naziv, out var v) ? v?.Tekst : null;
    }

    public double? GetBroj(string naziv)
    {
        return Atributi.TryGetValue(naziv, out var v) ? v?.Broj : null;
    }
}


public class MojDataSet
{
    public List<string> Historija { get; set; } = [];
    public List<RedPodatka> Podaci { get; set; } = new();
    public List<AtributMeta> Atributi { get; set; } = new();
    public string? CiljnaKolona { get; set; }

    public MojDataSet(IEnumerable<string> historija, List<RedPodatka> podaci, List<AtributMeta> atributi, string? ciljnaVarijabla = null)
    {
        if (podaci.Count == 0)
            throw new ArgumentException("Podaci ne mogu biti prazni.");
        Podaci = podaci;
        podaci.ForEach(red => red.MojDataSet = this); // Postavljamo MojDataSet za svaki red
        this.Historija = historija.ToList();
        Atributi = atributi;
        IskljuciAtribute("ID"); // Isključujemo ID ako postoji
        if (!string.IsNullOrWhiteSpace(ciljnaVarijabla))
        {
            SetCiljnaVarijabla(ciljnaVarijabla);
        }
        Analiziraj();
    }

    public void SetCiljnaVarijabla(string? ciljnaVarijabla)
    {
        if (string.IsNullOrWhiteSpace(ciljnaVarijabla))
            throw new ArgumentException("Ciljna kolona ne može biti prazna.");

        var meta = Atributi.SingleOrDefault(a => a.Naziv == ciljnaVarijabla);
        if (meta == null)
            throw new ArgumentException($"Atribut '{ciljnaVarijabla}' ne postoji u metapodacima.");

        meta.KoristiZaModel = false;
        CiljnaKolona = ciljnaVarijabla;
        DodajHistorijskiZapis($"Određena ciljna varijabla  ---> '{ciljnaVarijabla}'");
    }

    public MojDataSet DodajHistorijskiZapis(string historijskiZapis)
    {
        this.Historija.Add(historijskiZapis);
        return this;
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
            new MojDataSet(Historija, trening, Atributi, CiljnaKolona).DodajHistorijskiZapis($"Kreiran trening set {(1 - testProcenat):P0}"),
            new MojDataSet(Historija, test, Atributi, CiljnaKolona).DodajHistorijskiZapis($"Kreiran test set {testProcenat:P0}")
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
        rezultat.DodatniInfo = klasifikator.DodatniInfo;

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
        DodajHistorijskiZapis($"Nova kategorijska kolona ---> '{nazivKolone}'");
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
        DodajHistorijskiZapis($"Nova numerička kolona ---> '{nazivKolone}'");
    }

    public void Analiziraj()
    {
        foreach (var meta in Atributi)
        {
            if (meta.TipAtributa == TipAtributa.Kategoricki)
            {
                var tekstualne = MojDataSetHelper.DohvatiKategorije(Podaci, meta.Naziv, sortiraj: false);

                meta.Kategoricki = new KategorickiInfo
                {
                    BrojRazlicitihVrijednosti = tekstualne.Count(),
                    Top5Najcescih = DistribucijaHelper.TopVrijednosti(tekstualne, 5)
                        .Select(t => new VrijednostBrojac
                        {
                            Vrijednost = t.vrijednost,
                            BrojPojavljivanja = t.count
                        })
                        .ToList()
                };
            }
            else if (meta.TipAtributa == TipAtributa.Numericki)
            {
                List<double> brojevi = MojDataSetHelper.DohvatiBrojeve(Podaci, meta.Naziv, sortiraj: true);

                if (brojevi.Count > 0)
                {
                    var (min, max, srednja, std) = StatistikaHelper.Statistika(brojevi);
                    var medijana = MedianHelper.IzracunajMedijan(brojevi);

                    meta.Numericki = new NumerickiInfo
                    {
                        Min = min,
                        Max = max,
                        SrednjaVrijednost = srednja,
                        StandardnaDevijacija = std,
                        Medijana = medijana
                    };
                }
            }
        }
    }

    public void TransformirajKolonuNumericku(string nazivKolone, Func<double?, double?> transformacija)
    {
        int updated = 0;
        foreach (var red in Podaci)
        {
            if (red.Atributi.TryGetValue(nazivKolone, out var attr) && attr.TipAtributa == TipAtributa.Numericki)
            {
                double? stara = attr.Broj;
       
                double? nova = transformacija(stara);
                if (nova != stara)
                {
                    red.Atributi[nazivKolone] = VrijednostAtributa.NapraviNumericki(nova);
                    updated++;
                }
            }
        }

        DodajHistorijskiZapis($"Transformisana numerička kolona '{nazivKolone}' putem funkcije. Modifikovano: {updated}");
    }

    public void ImputirajNumerickuKolonuPoGrupi(string nazivKolone, string grupnaKolona1, string grupnaKolona2)
    {
        var grupe = GrupisanjeHelper.GrupisiPo2Kolone(Podaci, grupnaKolona1, grupnaKolona2);

        var medijani = grupe.ToDictionary(
            g => g.Key,
            g => MedianHelper.IzracunajMedijan(MojDataSetHelper.DohvatiBrojeve(g.Value, nazivKolone))
        );

        int brojModifikovanih = 0;

        foreach (var red in Podaci)
        {
            var key = (
                red.GetText(grupnaKolona1) ?? "__NULL__",
                red.GetText(grupnaKolona2) ?? "__NULL__"
            );

            if (red.GetBroj(nazivKolone) == null &&
                medijani.TryGetValue(key, out double? medijan) &&
                medijan.HasValue)
            {
                red.Atributi[nazivKolone] = VrijednostAtributa.NapraviNumericki(medijan);
                brojModifikovanih++;
            }
        }

        DodajHistorijskiZapis($"Transformisana numerička kolona '{nazivKolone}' imputacijom po grupama '{grupnaKolona1}', '{grupnaKolona2}'. Modifikovano: {brojModifikovanih}");
    }


}