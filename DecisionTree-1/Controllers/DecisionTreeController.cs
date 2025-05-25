namespace DecisionTree_1.Controllers;

using DecisionTree.Model.DataSet;
using DecisionTree.Model.Helper;
using DecisionTree.Model.Model;
using Microsoft.AspNetCore.Mvc;
using static StabloKlasifikator;

[ApiController]
[Route("api/[controller]/[action]")]
public class DecisionTreeController : ControllerBase
{
    private readonly ExcelHelper _ucitavac;

    public DecisionTreeController(ExcelHelper ucitavac)
    {
        _ucitavac = ucitavac;
    }

    [HttpGet]
    public IActionResult Play()
    {
        return Pokreni(new()
        {
            PutanjaDoFajla = "Files/play1.xlsx",
            CiljnaVarijabla = "Play",
            TestProcenat = 0.2,
            KlasifikatorParamteri = new (){
                MaxDepth = 5,
                MinSamples = 5
            }
        });
    }

    [HttpGet]
    public IActionResult Cardio()
    {
        return Pokreni(new()
        {
            PutanjaDoFajla = "Files/cardio1.xlsx",
            CiljnaVarijabla = "dcRyth",
            TestProcenat = 0.3,
            KlasifikatorParamteri = new()
            {
                MaxDepth = 5,
                MinSamples = 5
            }
        });
    }

    [HttpGet]
    public IActionResult Sales()
    {
        StabloZahtjev zahtjev = new()
        {
            PutanjaDoFajla = "Files/Sales3.xlsx",
            CiljnaVarijabla = "SalesCategory",
            TestProcenat = 0.2,
            KlasifikatorParamteri = new()
            {
                MaxDepth = 5,
                MinSamples = 5
            }
        };

        MojDataSet fullDataSet = _ucitavac.Ucitaj(zahtjev.PutanjaDoFajla);

        var (q1, q3) = KvartilaHelper.IzracunajKvartile(fullDataSet.Podaci, "OutletSales");

        fullDataSet.DodajKolonuKategorijski("SalesCategory", red =>
        {
            if (!red.Atributi.TryGetValue("OutletSales", out var attr) || !attr.Broj.HasValue)
                return null;

            var val = attr.Broj.Value;
            if (val < q1) return "Low";
            if (val > q3) return "High";
            return "Medium";
        });

        fullDataSet.CiljnaKolona = "SalesCategory";
        fullDataSet.IskljuciAtribute("OutletSales");

        //List<double> weightKolona = MojDataSetHelper.DohvatiBrojeve(fullDataSet.Podaci, "Weight");
        //double? medijana = MedianHelper.IzracunajMedijan(weightKolona);
        //fullDataSet.TransformirajKolonuNumericku("Weight", x => x ?? medijana);

        fullDataSet.ImputirajNumerickuKolonuPoGrupi("Weight", "ProductType", "OutletType");

        (MojDataSet treningSet, MojDataSet testSet) = fullDataSet.Podijeli(zahtjev.TestProcenat, random_state: 42);

        StabloKlasifikator stablo = new StabloKlasifikator(treningSet, zahtjev.KlasifikatorParamteri);
        GraphvizVisualizer.MakeDotFile(stablo.korijen, "Files/Sales3");

        EvaluacijaRezultat rezultat = fullDataSet.Evaluiraj(stablo, testSet);

        return Ok(new
        {
            rezultat,
        });
    }

    [HttpGet]
    public IActionResult Mushroom()
    {
        return Pokreni(new()
        {
            PutanjaDoFajla = "Files/mushroom1.xlsx",
            CiljnaVarijabla = "class",
            TestProcenat = 0.2,
            KlasifikatorParamteri = new()
            {
                MaxDepth = 6,
                MinSamples = 6
            }
        });
    }

    [HttpGet]
    public IActionResult AdultCensusIncome()
    {
        return Pokreni(new()
        {
            PutanjaDoFajla = "Files/adult-census-income.xlsx",
            CiljnaVarijabla = "class",
            TestProcenat = 0.2,
            KlasifikatorParamteri = new()
            {
                MaxDepth = 8,
                MinSamples = 4
            }
        });
    }
    [HttpGet]
    public IActionResult Pokreni(
        [FromQuery] StabloZahtjev zahtjev)
    {
        MojDataSet fullDataSet = _ucitavac.Ucitaj(zahtjev.PutanjaDoFajla, zahtjev.CiljnaVarijabla);
        (MojDataSet treningSet, MojDataSet testSet) = fullDataSet.Podijeli(zahtjev.TestProcenat, random_state: 42);

        StabloKlasifikator stablo = new StabloKlasifikator(treningSet, zahtjev.KlasifikatorParamteri);
        EvaluacijaRezultat rezultat = fullDataSet.Evaluiraj(stablo, testSet);

        return Ok(new
        {
            rezultat,
        });
    }

    public class StabloZahtjev
    {
        public string PutanjaDoFajla { get; set; } = string.Empty;

        public string CiljnaVarijabla { get; set; } = string.Empty;

        public double TestProcenat { get; set; } = 0.2;

        public StabloKlasifikatorParamteri KlasifikatorParamteri { get; set; } = new();
    }
}
