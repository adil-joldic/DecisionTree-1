using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTree.Model.Model
{
    public class KvartilaHelper
    {
        public static (double q1, double q3) IzracunajKvartile(List<RedPodatka> podaci, string nazivKolone)
        {
            List<double> vrijednosti = podaci
                .Select(p => p.Atributi.TryGetValue(nazivKolone, out var v) ? v?.Broj : null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .OrderBy(x => x)
                .ToList();

            if (!vrijednosti.Any())
                throw new InvalidOperationException($"Kolona '{nazivKolone}' ne sadrži numeričke vrijednosti.");

            int count = vrijednosti.Count;
            double q1 = vrijednosti[count / 4];
            double q3 = vrijednosti[(3 * count) / 4];

            return (q1, q3);
        }
    }
}
