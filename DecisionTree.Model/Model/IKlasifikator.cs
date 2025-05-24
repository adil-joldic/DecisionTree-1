namespace DecisionTree.Model.Model;

public abstract class IKlasifikator
{
    public abstract string Naziv { get; }
    public abstract object Parametri { get; }
    public double VrijemeTreniranjaSek { get; init; }

    /// <summary>
    /// Vraća naziv klase na osnovu atributa.
    /// </summary>
    /// <param name="atributi">Ulazni atributi podatka</param>
    /// <returns>Naziv predikcije (klase)</returns>
    public abstract string Predikcija(RedPodatka red);
}
