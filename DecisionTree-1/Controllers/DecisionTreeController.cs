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
    public IActionResult Play()
    {
        string putanjaDoFajla = "Files/play1.xlsx";
        string ciljnaVarijabla = "Play";

        return Pokreni(putanjaDoFajla, ciljnaVarijabla);
    }

    [HttpGet]
    public IActionResult Sales()
    {
        string putanjaDoFajla = "Files/Sales4.xlsx";
        string ciljnaVarijabla = "SalesCategory";

        return Pokreni(putanjaDoFajla, ciljnaVarijabla);
    }

    [HttpGet]
    public IActionResult Pokreni([FromQuery] string putanjaDoFajla, [FromQuery] string ciljnaVarijabla, [FromQuery] double testProcenat = 0.2)
    {
        MojDataSet fullDataSet = _ucitavac.Ucitaj(putanjaDoFajla, ciljnaVarijabla);

        (MojDataSet treningSet, MojDataSet testSet) = fullDataSet.Podijeli(testProcenat);

        StabloKlasifikator stablo = new StabloKlasifikator(treningSet);

        EvaluacijaRezultat rezultat  = fullDataSet.Evaluiraj(stablo, testSet);

        return Ok(new
        {
            rezultat
        });
    }
}
