namespace DecisionTree_1.Controllers;

using DecisionTree.Model.Helper;
using Microsoft.AspNetCore.Mvc;
using static StabloKlasifikator;

[ApiController]
[Route("api/[controller]/[action]")]
public class DecisionTreeController : ControllerBase
{
    private readonly ExcelAlati _ucitavac;

    public DecisionTreeController(ExcelAlati ucitavac)
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
        return Pokreni(new()
        {
            PutanjaDoFajla = "Files/Sales3.xlsx",
            CiljnaVarijabla = "SalesCategory",
            TestProcenat = 0.2,
            KlasifikatorParamteri = new()
            {
                MaxDepth = 5,
                MinSamples = 5
            }
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

        var stopwatchTreniranje = System.Diagnostics.Stopwatch.StartNew();

            StabloKlasifikator stablo = new StabloKlasifikator(treningSet, zahtjev.KlasifikatorParamteri);

        stopwatchTreniranje.Stop();

        var stopwatchEvaluacija = System.Diagnostics.Stopwatch.StartNew();

            EvaluacijaRezultat rezultat = fullDataSet.Evaluiraj(stablo, testSet);

        stopwatchEvaluacija.Stop();


        return Ok(new
        {
            vrijemeTreniranja_ms = stopwatchTreniranje.ElapsedMilliseconds,
            vrijemeEvaluacije_ms = stopwatchEvaluacija.ElapsedMilliseconds,
            ukupnoVrijeme_ms = stopwatchTreniranje.ElapsedMilliseconds + stopwatchEvaluacija.ElapsedMilliseconds,
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
