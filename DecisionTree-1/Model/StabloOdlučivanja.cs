/// <summary>
/// Predstavlja implementaciju stabla odlučivanja za klasifikaciju.
/// Koristi Gini indeks za izbor najboljeg atributa pri svakom grananju.
/// </summary>
public class StabloOdlučivanja
{
    /// <summary>
    /// Rekurzivno gradi stablo odlučivanja iz skupa podataka.
    /// </summary>
    public CvorStabla IzgradiStablo(List<RedPodatka> podaci, List<string> atributi)
    {
        // Ako sve instance pripadaju istoj klasi, vrati list čvor s tom klasom
        if (podaci.Select(p => p.Klasa).Distinct().Count() == 1)
            return new CvorStabla { Klasa = podaci[0].Klasa };

        // Ako nema više atributa za grananje, vrati najčešću klasu
        if (!atributi.Any())
            return new CvorStabla { Klasa = NajcescaKlasa(podaci) };

        // Odaberi najbolji atribut za grananje
        string najboljiAtribut = IzaberiNajboljiAtribut(podaci, atributi);
        var cvor = new CvorStabla { Atribut = najboljiAtribut };

        // Gradi podstabla za svaku vrijednost atributa
        var vrijednosti = podaci.Select(p => p.Atributi[najboljiAtribut]).Distinct();
        foreach (var vrijednost in vrijednosti)
        {
            var podskup = podaci.Where(p => p.Atributi[najboljiAtribut] == vrijednost).ToList();
            if (!podskup.Any())
            {
                cvor.Djeca[vrijednost] = new CvorStabla { Klasa = NajcescaKlasa(podaci) };
            }
            else
            {
                var preostaliAtributi = atributi.Where(a => a != najboljiAtribut).ToList();
                cvor.Djeca[vrijednost] = IzgradiStablo(podskup, preostaliAtributi);
            }
        }

        return cvor;
    }

    /// <summary>
    /// Vraća najčešće pojavljivanu klasu u datom skupu podataka.
    /// </summary>
    private string NajcescaKlasa(List<RedPodatka> podaci) =>
        podaci.GroupBy(p => p.Klasa)
              .OrderByDescending(g => g.Count())
              .First().Key;

    /// <summary>
    /// Odabire najbolji atribut za grananje na osnovu Gini indeksa.
    /// </summary>
    private string IzaberiNajboljiAtribut(List<RedPodatka> podaci, List<string> atributi)
    {
        double najboljiGini = double.MaxValue;
        string najboljiAtribut = atributi[0];

        foreach (var atribut in atributi)
        {
            double gini = IzracunajGiniIndeks(podaci, atribut);
            if (gini < najboljiGini)
            {
                najboljiGini = gini;
                najboljiAtribut = atribut;
            }
        }

        return najboljiAtribut;
    }

    /// <summary>
    /// Izračunava Gini indeks za dati atribut u skupu podataka.
    /// </summary>
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

    /// <summary>
    /// Na osnovu već izgrađenog stabla i ulaznih atributa, vraća predviđenu klasu.
    /// </summary>
    public string Predikcija(CvorStabla cvor, Dictionary<string, string> atributi)
    {
        if (cvor.JeList)
            return cvor.Klasa!;

        var vrijednost = atributi[cvor.Atribut!];
        if (cvor.Djeca.TryGetValue(vrijednost, out var dijete))
            return Predikcija(dijete, atributi);
        else
            return "Nepoznato";
    }
}
