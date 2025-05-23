namespace DecisionTree.Model.Model;

public interface IKlasifikator
{
    /// <summary>
    /// Vraća naziv klase na osnovu atributa.
    /// </summary>
    /// <param name="atributi">Ulazni atributi podatka</param>
    /// <returns>Naziv predikcije (klase)</returns>
    string Predikcija(RedPodatka red);
}
