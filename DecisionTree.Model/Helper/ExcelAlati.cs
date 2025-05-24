using OfficeOpenXml;

namespace DecisionTree.Model.Helper;

public class ExcelAlati
{
    public MojDataSet Ucitaj(string putanja, string nazivCiljneKolone)
    {
        if (!File.Exists(putanja))
            throw new Exception("Excel fajl nije pronađen.");

        nazivCiljneKolone = nazivCiljneKolone.ToUpper();

        ExcelPackage.License.SetNonCommercialOrganization("FIT education");

        using var paket = new ExcelPackage(new FileInfo(putanja));
        var lista = paket.Workbook.Worksheets[0];

        int brojKolona = lista.Dimension.Columns;
        int brojRedova = lista.Dimension.Rows;

        var zaglavlje = new List<string>();
        for (int i = 1; i <= brojKolona; i++)
            zaglavlje.Add(lista.Cells[1, i].Text.Trim());

        var privremeniPodaci = new List<Dictionary<string, string>>();
        var klase = new List<string>();

        for (int i = 2; i <= brojRedova; i++)
        {
            var red = new Dictionary<string, string>();
            string? klasa = null;

            for (int j = 1; j <= brojKolona; j++)
            {
                string naziv = zaglavlje[j - 1].ToUpper();
                string vrijednost = lista.Cells[i, j].Text.Trim();

                if (naziv == nazivCiljneKolone)
                    klasa = vrijednost;
                else
                    red[naziv] = vrijednost;
            }

            if (!string.IsNullOrWhiteSpace(klasa))
            {
                privremeniPodaci.Add(red);
                klase.Add(klasa);
            }
        }

        var atributiMeta = zaglavlje
            .Where(n => n != nazivCiljneKolone)
            .Select(n => new AtributMeta
            {
                Naziv = n.ToUpper(),
                TipAtributa = privremeniPodaci
                    .Where(p => p.ContainsKey(n) && !string.IsNullOrWhiteSpace(p[n]))
                    .All(p => double.TryParse(p[n], out _))
                        ? TipAtributa.Numericki
                        : TipAtributa.Kategoricki
            })
            .ToList();

        var redovi = new List<RedPodatka>();

        for (int i = 0; i < privremeniPodaci.Count; i++)
        {
            var red = new RedPodatka
            {
                Klasa = klase[i],
                Atributi = new Dictionary<string, VrijednostAtributa>()
            };

            foreach (var meta in atributiMeta)
            {
                string vrijednost = privremeniPodaci[i].TryGetValue(meta.Naziv, out var v) ? v : "";

                if (meta.TipAtributa == TipAtributa.Kategoricki)
                {
                    red.Atributi[meta.Naziv] = VrijednostAtributa.NapraviKategorijski(vrijednost);
                }
                else if (meta.TipAtributa == TipAtributa.Numericki)
                {
                    double.TryParse(vrijednost, out var broj);
                    red.Atributi[meta.Naziv] = VrijednostAtributa.NapraviNumericki(broj);
                }
            }

            redovi.Add(red);
        }

        return new MojDataSet([putanja], redovi, atributiMeta, nazivCiljneKolone);
    }

}
