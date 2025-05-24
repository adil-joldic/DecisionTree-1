using OfficeOpenXml;

namespace DecisionTree.Model.Helper;

public class ExcelAlati
{
    public MojDataSet Ucitaj(string putanja, string nazivCiljneKolone)
    {
        if (!File.Exists(putanja))
            throw new Exception("Excel fajl nije pronađen.");               

        ExcelPackage.License.SetNonCommercialOrganization("FIT education");

        using var paket = new ExcelPackage(new FileInfo(putanja));
        var lista = paket.Workbook.Worksheets[0];

        var zaglavlje = new List<string>();
        int brojKolona = lista.Dimension.Columns;
        int brojRedova = lista.Dimension.Rows;

        for (int i = 1; i <= brojKolona; i++)
            zaglavlje.Add(lista.Cells[1, i].Text.Trim());

        // Privremeno skladišti raw podatke kao Dictionary<string, string>
        List<Dictionary<string, string>> privremeniPodaci = new ();
        List<string> klase = new ();

        for (int i = 2; i <= brojRedova; i++)
        {
            Dictionary<string, string> red = new ();
            string klasa = "";

            for (int j = 1; j <= brojKolona; j++)
            {
                string naziv = zaglavlje[j - 1];
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

        // Odredi tipove atributa
        List<AtributMeta> atributiMeta = zaglavlje
            .Where(n => n != nazivCiljneKolone)
            .Select(n => new AtributMeta
            {
                Naziv = n,
                TipAtributa = privremeniPodaci
                    .Where(p => p.ContainsKey(n) && !string.IsNullOrWhiteSpace(p[n]))
                    .All(p => double.TryParse(p[n], out _))
                        ? TipAtributa.Numericki
                        : TipAtributa.Kategoricki
            })
            .ToList();

        // Konvertuj u RedPodatka s ispravnim VrijednostAtributa
        List<RedPodatka> redovi = new ();

        for (int i = 0; i < privremeniPodaci.Count; i++)
        {
            var red = new RedPodatka
            {
                Atributi = new Dictionary<string, VrijednostAtributa>(),
                Klasa = klase[i]
            };

            foreach (var meta in atributiMeta)
            {
                string vrijednost = privremeniPodaci[i].ContainsKey(meta.Naziv)
                    ? privremeniPodaci[i][meta.Naziv]
                    : "";

                red.Atributi[meta.Naziv] = new VrijednostAtributa(vrijednost, meta.TipAtributa);
            }

            redovi.Add(red);
        }

        var dataSetNaziv = Path.GetFileNameWithoutExtension(putanja);

        return new MojDataSet(dataSetNaziv, redovi, atributiMeta, nazivCiljneKolone);
    }
}
