using DecisionTree.Model.Model;

public class CvorStabla
{
    /// <summary>
    /// Atribut po kojem se vrši grananje u ovom čvoru.
    /// Ako je čvor list, ova vrijednost je null.
    /// </summary>
    public string? Atribut { get; set; }

    /// <summary>
    /// Naziv ciljne klase ako je čvor list (npr. "Play", "Not play").
    /// Ako nije list, ova vrijednost je null.
    /// </summary>
    public string? Klasa { get; set; }

    /// <summary>
    /// Djeca ovog čvora predstavljaju moguće vrijednosti atributa.
    /// Ključ je vrijednost atributa (npr. "outlook"), a vrijednost je podstablo.
    /// </summary>
    public Dictionary<string, CvorStabla> Djeca { get; set; } = new();

    /// <summary>
    /// Indikator da li je čvor list (nema više grananja).
    /// </summary>
    public bool JeList => Klasa != null;
}


/// <summary>
/// Predstavlja implementaciju stabla odlučivanja za klasifikaciju.
/// Koristi Gini indeks i podržava samo kategorijske atribute.
/// </summary>
public class StabloKlasifikator : IKlasifikator
{
    private readonly CvorStabla _korijen;

    public StabloKlasifikator(MojDataSet podaci)
    {
        _korijen = IzgradiStablo(podaci.Podaci, podaci.Atributi);
    }

    /// <summary>
    /// Rekurzivno gradi stablo odlučivanja iz skupa podataka koristeći samo kategorijske atribute.
    /// </summary>
    public CvorStabla IzgradiStablo(List<RedPodatka> podaci, List<AtributMeta> atributi)
    {
        // Ako svi redovi imaju istu klasu – vrati list sa tom klasom
        if (podaci.Select(p => p.Klasa).Distinct().Count() == 1)
            return new CvorStabla { Klasa = podaci[0].Klasa };

        // Ako nema atributa za grananje – vrati najčešću klasu
        if (!atributi.Any())
            return new CvorStabla { Klasa = NajcescaKlasa(podaci) };

        // Ukloni numeričke atribute
        var kandidati = atributi.Where(a => a.Tip == TipAtributa.Kategorijski).ToList();
        if (!kandidati.Any())
            return new CvorStabla { Klasa = NajcescaKlasa(podaci) };

        // Odaberi najbolji atribut (po Gini indeksu)
        var najbolji = kandidati
            .Select(a => new { Atribut = a.Naziv, Gini = IzracunajGiniIndeks(podaci, a.Naziv) })
            .OrderBy(x => x.Gini)
            .First();

        var cvor = new CvorStabla { Atribut = najbolji.Atribut };

        // Gradi podstabla za svaku vrijednost atributa
        var vrijednosti = podaci.Select(p => p.Atributi[najbolji.Atribut]).Distinct();
        foreach (var vrijednost in vrijednosti)
        {
            var podskup = podaci.Where(p => p.Atributi[najbolji.Atribut] == vrijednost).ToList();
            var preostali = kandidati.Where(a => a.Naziv != najbolji.Atribut).ToList();

            cvor.Djeca[vrijednost] = podskup.Any()
                ? IzgradiStablo(podskup, preostali)
                : new CvorStabla { Klasa = NajcescaKlasa(podaci) };
        }

        return cvor;
    }

    private string NajcescaKlasa(List<RedPodatka> podaci) =>
        podaci.GroupBy(p => p.Klasa)
              .OrderByDescending(g => g.Count())
              .First().Key;

    private double IzracunajGiniIndeks(List<RedPodatka> podaci, string atribut)
    {
        var grupe = podaci.GroupBy(p => p.Atributi[atribut]);
        double ukupno = podaci.Count;
        double gini = 0.0;

        foreach (var grupa in grupe)
        {
            double velicina = grupa.Count();
            double skor = grupa.GroupBy(p => p.Klasa)
                                .Select(g => Math.Pow(g.Count() / velicina, 2))
                                .Sum();
            gini += (1 - skor) * (velicina / ukupno);
        }

        return gini;
    }
    public string Predikcija(Dictionary<string, string> atributi)
    {
        return PredikcijaRekurzivno(_korijen, atributi);
    }

    private string PredikcijaRekurzivno(CvorStabla cvor, Dictionary<string, string> atributi)
    {
        if (cvor.JeList)
            return cvor.Klasa!;

        if (!atributi.ContainsKey(cvor.Atribut!))
            return "Nepoznato";

        var vrijednost = atributi[cvor.Atribut!];
        if (cvor.Djeca.TryGetValue(vrijednost, out var dijete))
            return PredikcijaRekurzivno(dijete, atributi);
        else
            return "Nepoznato";
    }
}
