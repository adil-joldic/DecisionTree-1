namespace DecisionTree_1.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]/[action]")]
public class DecisionTreeController : ControllerBase
{
    private readonly ExcelUcitaj _ucitavac;

    public DecisionTreeController(ExcelUcitaj ucitavac)
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
    public IActionResult Pokreni([FromQuery] string file, [FromQuery] string target)
    {
        MojDataSet skup = _ucitavac.Ucitaj(file, target);
        (List<RedPodatka> trening, List<RedPodatka> test) = skup.Podijeli();
        CvorStabla stablo = new StabloOdlučivanja().IzgradiStablo(trening, skup.Atributi);
        double tacnost = skup.IzracunajTacnost(stablo, test);

        return Ok(new
        {
            Tacnost = $"{tacnost * 100:F2}%",
            UkupnoTestiranih = test.Count,
            UspjesnoPredvidjeno = (int)(tacnost * test.Count)
        });
    }
}
