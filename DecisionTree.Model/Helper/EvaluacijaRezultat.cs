namespace DecisionTree.Model.Helper;

public class EvaluacijaRezultat
{
    //https://medium.com/analytics-vidhya/confusion-matrix-accuracy-precision-recall-f1-score-ade299cf63cd

    public double Accuracy { get; set; }

    // TODO: Dodati detaljne evaluacione metrike
    public Dictionary<string, double> Precision { get; set; } = new(); // po klasama
    public Dictionary<string, double> Recall { get; set; } = new(); // po klasama
    public Dictionary<string, double> F1Score { get; set; } = new(); // po klasama
    public Dictionary<(string Stvarna, string Predikcija), int> ConfusionMatrix { get; set; } = new();
    public Dictionary<string, int> SveKlase { get; set; } = new(); // broj stvarnih po klasama

    public void Ispisi()
    {
        /**
            Tačnost (Accuracy): 88.24%

            🔹 Klasa: Igraj
               - Precision: 91.67%
               - Recall:    84.62%
               - F1 Score:  88.00%

            🔹 Klasa: Ne igraj
               - Precision: 85.00%
               - Recall:    92.31%
               - F1 Score:  88.52%

            🧾 Confusion Matrix:
            Stvarna \ Predikcija  Igraj   Ne igraj
            Igraj                   22       4
            Ne igraj                 2      24

         */

        Console.WriteLine($"Tačnost (Accuracy): {Accuracy:P2}");
        Console.WriteLine();

        var klase = SveKlase.Keys.OrderBy(k => k).ToList();
        foreach (var klasa in klase)
        {
            Console.WriteLine($" Klasa: {klasa}");
            Console.WriteLine($"   - Precision: {Precision.GetValueOrDefault(klasa):P2}");
            Console.WriteLine($"   - Recall:    {Recall.GetValueOrDefault(klasa):P2}");
            Console.WriteLine($"   - F1 Score:  {F1Score.GetValueOrDefault(klasa):P2}");
            Console.WriteLine();
        }

        Console.WriteLine("🧾 Confusion Matrix:");
        Console.Write("Stvarna \\ Predikcija\t");
        foreach (var k in klase)
            Console.Write($"{k}\t");
        Console.WriteLine();

        foreach (var stvarna in klase)
        {
            Console.Write($"{stvarna}\t\t");
            foreach (var predikcija in klase)
            {
                ConfusionMatrix.TryGetValue((stvarna, predikcija), out int count);
                Console.Write($"{count}\t");
            }
            Console.WriteLine();
        }
    }
}