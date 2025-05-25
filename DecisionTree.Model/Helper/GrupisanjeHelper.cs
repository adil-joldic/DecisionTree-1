namespace DecisionTree.Model.Helper;

public static class GrupisanjeHelper
{
    /// <summary>
    ///     Grupisanje redova po dvije kategorijske kolone.
    /// </summary>
    public static Dictionary<(string?, string?), List<RedPodatka>> GrupisiPo2Kolone(List<RedPodatka> podaci, string kolona1, string kolona2)
    {
        return podaci
            .GroupBy(r => (
                r.Atributi.TryGetValue(kolona1, out var v1) ? v1?.Tekst : null,
                r.Atributi.TryGetValue(kolona2, out var v2) ? v2?.Tekst : null))
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    ///     Grupisanje redova po jednoj kategorijskoj koloni.
    /// </summary>
    public static Dictionary<string, List<RedPodatka>> GrupisiPo1Koloni(List<RedPodatka> podaci, string kolona)
    {
        return podaci
            .GroupBy(r =>
            {
                if (r.Atributi.TryGetValue(kolona, out var v) && v?.Tekst is string tekst)
                    return tekst;
                return "NEPOZNATO"; // fallback vrijednost za nedostajuće
            })
            .ToDictionary(g => g.Key, g => g.ToList());
    }


}
