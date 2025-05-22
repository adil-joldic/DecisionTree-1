using DecisionTree.Model.Helper;
using DecisionTree.Model.Model;

public enum TipAtributa
{
    Kategorijski,
    Numerički
}

public class AtributMeta
{
    public string Naziv { get; set; } = string.Empty;
    public TipAtributa Tip { get; set; } = TipAtributa.Kategorijski;
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
            .Select(kv => new AtributMeta
            {
                Naziv = kv.Key,
                Tip = podaci.All(p => double.TryParse(p.Atributi[kv.Key], out _))
                        ? TipAtributa.Numerički
                        : TipAtributa.Kategorijski
            }).ToList();
    }

    // ✂️ Funkcija za nasumično dijeljenje na trening i test skup
    public (MojDataSet Train, MojDataSet Test) Podijeli(double testProcenat = 0.2, int? random_state = null)
    {
        // todo: za isti random_stati treba dobiti isti rezultat
        // ako je random_state null, koristi se trenutni sistemski random

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
        int tacni = 0;

        foreach (var red in testSkup.Podaci)
        {
            var predikcija = klasifikator.Predikcija(red.Atributi);
            if (predikcija == red.Klasa)
                tacni++;
        }

        var rezultat = new EvaluacijaRezultat
        {
            Accuracy = (double)tacni / testSkup.Podaci.Count,

            // TODO: Implementirati sljedeće metrike
            Precision = new (),
            Recall = new(),
            F1Score = new(),
            ConfusionMatrix = new(),
            SveKlase = new ()
        };

        return rezultat;
    }
}
