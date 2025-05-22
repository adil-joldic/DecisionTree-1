namespace DecisionTree_1.Controllers;

using DecisionTree.Model.Helper;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult Play1()
    {
        string file = "Files/play1.xlsx";
        string target = "Play";

        return Pokreni(file, target);
    }

    [HttpGet]
    public IActionResult Sales()
    {
        string file = "Files/Sales3.xlsx";
        string target = "OutletSize";

        return Pokreni(file, target);
    }

    [HttpGet]
    public IActionResult Pokreni([FromQuery] string file, [FromQuery] string target, [FromQuery] double testProcenat = 0.2)
    {
        MojDataSet fullDataSet = _ucitavac.Ucitaj(file, target);

        (MojDataSet? treningSet, MojDataSet? testSet) = fullDataSet.Podijeli(testProcenat);

        StabloKlasifikator stablo = new StabloKlasifikator(treningSet);

        var x  = fullDataSet.Evaluiraj(stablo, testSet);

        return Ok(new
        {
            Tacnost = $"{x.Accuracy * 100:F2}%",
            UkupnoTestiranih = testSet.Podaci.Count,
            UspjesnoPredvidjeno = (int)(x.Accuracy * testSet.Podaci.Count)
        });
    }
}
