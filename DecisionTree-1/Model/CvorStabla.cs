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
