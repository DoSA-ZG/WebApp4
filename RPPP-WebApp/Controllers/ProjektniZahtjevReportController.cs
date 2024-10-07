using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers;

public class ProjektniZahtjevReportController : Controller
{
    private readonly Rppp04Context ctx;
    private readonly IWebHostEnvironment environment;
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>
    /// Konstruktor za upravljač izvještajima suradnika
    /// </summary>
    /// <param name="ctx">kontekst za spajanje na bazu</param>
    /// <param name="environment">okruženje</param>
    public ProjektniZahtjevReportController(Rppp04Context ctx, IWebHostEnvironment environment)
    {
        this.ctx = ctx;
        this.environment = environment;
    }
    
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> ProjektniZahtjeviExcel()
        {
            var projektniZahtjevi = await ctx.ProjektniZahtjev
                                     .Include(s => s.Projekt)
                                     .Include(s => s.VrstaZahtjeva)
                                     .Include(s => s.PrioritetZahtjeva)
                                     .AsNoTracking()
                                     .OrderBy(p => p.Id)
                                     .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis projektnih zahtjeva";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Projektni zahtjevi");

                worksheet.Cells[1, 1].Value = "Projekt";
                worksheet.Cells[1, 2].Value = "Naziv zahtjeva";
                worksheet.Cells[1, 3].Value = "Opis zahtjeva";
                worksheet.Cells[1, 4].Value = "Prioritet stupanj";
                worksheet.Cells[1, 5].Value = "Prioritet naziv";
                worksheet.Cells[1, 6].Value = "Vrsta zahtjeva";

                for (int i = 0; i < projektniZahtjevi.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = projektniZahtjevi[i].Projekt.NazivProjekt;
                    worksheet.Cells[i + 2, 2].Value = projektniZahtjevi[i].NazivZahtjeva;
                    worksheet.Cells[i + 2, 3].Value = projektniZahtjevi[i].OpisZahtjeva;
                    worksheet.Cells[i + 2, 4].Value = projektniZahtjevi[i].PrioritetZahtjeva.StupanjPrioriteta;
                    worksheet.Cells[i + 2, 5].Value = projektniZahtjevi[i].PrioritetZahtjeva.NazivPrioritetaZahtjeva;
                    worksheet.Cells[i + 2, 6].Value = projektniZahtjevi[i].VrstaZahtjeva.NazivVrsteZahtjeva;
                }

                worksheet.Cells[1, 1, projektniZahtjevi.Count + 1, 6].AutoFitColumns();

                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "projektnizahtjevi.xlsx");
        }

        public async Task<IActionResult> VrsteZahtjevaExcel()
        {
            var vrsteZahtjeva = await ctx.VrstaZahtjeva
                                     .AsNoTracking()
                                     .OrderBy(d => d.Id)
                                     .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis vrsta zahtjeva";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Vrste Zahtjeva");

                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Naziv vrste zahtjeva";

                for (int i = 0; i < vrsteZahtjeva.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = vrsteZahtjeva[i].Id;
                    worksheet.Cells[i + 2, 2].Value = vrsteZahtjeva[i].NazivVrsteZahtjeva;

                }

                worksheet.Cells[1, 1, vrsteZahtjeva.Count + 1, 2].AutoFitColumns();

                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "vrsteZahtjeva.xlsx");
        }
        
    

    
}