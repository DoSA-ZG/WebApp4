using Azure;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.Model;
using PdfRpt.ColumnsItemsTemplates;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using RPPP_WebApp.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Azure.Identity;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za generiranje izvještaja u aplikaciji.
    /// </summary>
    public class ReportController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// Konstruktor za ReportController.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="environment">Hosting okruženje za aplikaciju.</param>
        public ReportController(Rppp04Context ctx, IWebHostEnvironment environment)
        {
            this.ctx = ctx;
            this.environment = environment;
        }

        /// <summary>
        /// Prikazuje početnu stranicu za izvještaje.
        /// </summary>
        /// <returns>Pogled za početnu stranicu izvještaja.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Generira Excel datoteku s popisom projekata.
        /// </summary>
        /// <returns>Excel datoteka s popisom projekata.</returns>
        public async Task<IActionResult> ProjektiExcel()
        {
            var projekti = await ctx.Projekt
                                     .Include(p => p.VrstaProjekta)
                                     .Include(p => p.Narucitelj)
                                     .Include(p => p.Dokument)
                                     .AsNoTracking()
                                     .OrderBy(p => p.NazivProjekt)
                                     .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis projekata";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Projekti");

                worksheet.Cells[1, 1].Value = "Naziv projekta";
                worksheet.Cells[1, 2].Value = "Kratica projekta";
                worksheet.Cells[1, 3].Value = "Vrsta projekta";
                worksheet.Cells[1, 4].Value = "Datum isporuke";
                worksheet.Cells[1, 5].Value = "Naručitelj";
                worksheet.Cells[1, 6].Value = "Dokumenti";

                for (int i = 0; i < projekti.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = projekti[i].NazivProjekt;
                    worksheet.Cells[i + 2, 2].Value = projekti[i].KraticaProjekt;
                    worksheet.Cells[i + 2, 3].Value = projekti[i].VrstaProjekta.Vrsta;

                    worksheet.Cells[i + 2, 4].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                    worksheet.Cells[i + 2, 4].Value = projekti[i].DatumIsporukaPr;

                    worksheet.Cells[i + 2, 5].Value = projekti[i].Narucitelj.NazivNarucitelj;

                    worksheet.Cells[i + 2, 6].Value = string.Join(", ", projekti[i].Dokument.Select(d => d.NazivDok));
                }

                worksheet.Cells[1, 1, projekti.Count + 1, 6].AutoFitColumns();

                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "projekti.xlsx");
        }

        /// <summary>
        /// Generira Excel datoteku s popisom dokumenata.
        /// </summary>
        /// <returns>Excel datoteka s popisom dokumenata.</returns>
        public async Task<IActionResult> DokumentiExcel()
        {
            var dokumenti = await ctx.Dokument
                                     .Include(d => d.IdVrstaDokNavigation)
                                     .Include(d => d.IdStatusDokNavigation)
                                     .Include(d => d.IdProjektNavigation)
                                     .AsNoTracking()
                                     .OrderBy(d => d.NazivDok)
                                     .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis dokumenata";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Dokumenti");

                worksheet.Cells[1, 1].Value = "Naziv dokumenta";
                worksheet.Cells[1, 2].Value = "Vrsta dokumenta";
                worksheet.Cells[1, 3].Value = "Status dokumenta";
                worksheet.Cells[1, 4].Value = "Vrijeme prijenosa";
                worksheet.Cells[1, 5].Value = "Datum zadnje izmjene";
                worksheet.Cells[1, 6].Value = "Projekt";

                for (int i = 0; i < dokumenti.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = dokumenti[i].NazivDok;
                    worksheet.Cells[i + 2, 2].Value = dokumenti[i].IdVrstaDokNavigation?.VrstaDok;
                    worksheet.Cells[i + 2, 3].Value = dokumenti[i].IdStatusDokNavigation?.StatusDok;

                    worksheet.Cells[i + 2, 4].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                    worksheet.Cells[i + 2, 4].Value = dokumenti[i].VrPrijenos;

                    worksheet.Cells[i + 2, 5].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                    worksheet.Cells[i + 2, 5].Value = dokumenti[i].DatumZadIzmj;

                    worksheet.Cells[i + 2, 6].Value = dokumenti[i].IdProjektNavigation.NazivProjekt;
                }

                worksheet.Cells[1, 1, dokumenti.Count + 1, 6].AutoFitColumns();

                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "dokumenti.xlsx");
        }

        /// <summary>
        /// Generira PDF izvještaj s popisom projekata.
        /// </summary>
        /// <returns>PDF izvještaj s popisom projekata.</returns>
        public async Task<IActionResult> Projekti()
        {
            string naslov = "Popis projekata";

            var projekti = from p in ctx.Projekt
                           join vp in ctx.VrstaProjekta on p.VrstaProjektaId equals vp.Id
                           join n in ctx.Narucitelj on p.NaruciteljId equals n.Id
                           join d in ctx.Dokument on p.Id equals d.ProjektId into dokumenti
                           select new ProjektiReportModel
                           {
                               IdProjekt = p.Id,
                               NazivProjekt = p.NazivProjekt,
                               Vrsta = vp.Vrsta,
                               DatumIsporukaPr = p.DatumIsporukaPr,
                               NazivNarucitelj = n.NazivNarucitelj,
                               NaziviDokumenata = ConcatenateDocumentNames(dokumenti)
                        };

            PdfReport report = CreateReport(naslov);

            #region Podnožje i zaglavlje
            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true);
                header.DefaultHeader(defaultHeader =>
                {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(naslov);
                });
            });
            #endregion
            #region Postavljanje izvora podataka i stupaca
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(projekti));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektiReportModel>(x => x.NazivProjekt);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Naziv projekta", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektiReportModel>(x => x.Vrsta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Vrsta projekta", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektiReportModel>(x => x.DatumIsporukaPr);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(1);
                    column.HeaderCell("Datum isporuke", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektiReportModel>(x => x.NazivNarucitelj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Naručitelj", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektiReportModel>(x => x.NaziviDokumenata);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(3);
                    column.HeaderCell("Dokumenti", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=projekti.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Spaja nazive dokumenata iz kolekcije u jedan string odvojen zarezima.
        /// </summary>
        /// <param name="dokumenti">Kolekcija dokumenata čiji će se nazivi spojiti.</param>
        /// <returns>String koji sadrži spojene nazive dokumenata odvojene zarezima.</returns>
        private static string ConcatenateDocumentNames(IEnumerable<Dokument> dokumenti)
        {

            var docs = dokumenti.ToList();

            if (docs != null && docs.Count > 0)
            {
                return string.Join(", ", docs.Select(d => d.NazivDok));
            }
            return string.Empty;
        }

        /// <summary>
        /// Generira PDF izvještaj s popisom dokumenata.
        /// </summary>
        /// <returns>PDF izvještaj s popisom dokumenata.</returns>
        public async Task<IActionResult> Dokumenti()
        {
            string naslov = "Popis dokumenata";

            var dokumenti = from d in ctx.Dokument
                            join vrd in ctx.VrstaDokumenta on d.VrstaDokumentaId equals vrd.Id
                            join std in ctx.StatusDokumenta on d.StatusDokumentaId equals std.Id
                            join p in ctx.Projekt on d.ProjektId equals p.Id
                            select new DokumentViewModel
                            {
                                Id = d.Id,
                                NazivDok = d.NazivDok,
                                VrstaDokumenta = vrd.VrstaDok,
                                StatusDokumenta = std.StatusDok,
                                VrPrijenos = d.VrPrijenos,
                                DatumZadIzmj = d.DatumZadIzmj,
                                NazivProjekt = p.NazivProjekt
                            };

            PdfReport report = CreateReport(naslov);

            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true); // It's a default setting to improve the performance.
                header.DefaultHeader(defaultHeader =>
                {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(naslov);
                });
            });
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(dokumenti));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<DokumentViewModel>(x => x.NazivDok);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(3);
                    column.HeaderCell("Naziv dokumenta", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<DokumentViewModel>(x => x.VrstaDokumenta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Vrsta dokumenta", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<DokumentViewModel>(x => x.StatusDokumenta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Status dokumenta", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<DokumentViewModel>(x => x.VrPrijenos);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(1);
                    column.HeaderCell("Vrijeme prijenosa", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<DokumentViewModel>(x => x.DatumZadIzmj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(1);
                    column.HeaderCell("Datum zadnje promjene", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<DokumentViewModel>(x => x.NazivProjekt);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Projekt", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=projekti.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Stvara i konfigurira instancu PdfReport za generiranje PDF izvješća.
        /// </summary>
        /// <param name="naslov">Naslov izvješća.</param>
        /// <returns>Instanca PdfReport za generiranje izvješća.</returns>
        private PdfReport CreateReport(string naslov)
        {
            var pdf = new PdfReport();

            pdf.DocumentPreferences(doc =>
            {
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "FER-ZPR",
                    Application = "RPPP04.MVC Core",
                    Title = naslov
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })

            .DefaultFonts(fonts => {
                fonts.Path(Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"),
                           Path.Combine(environment.WebRootPath, "fonts", "tahoma.ttf"));
                fonts.Size(9);
                fonts.Color(System.Drawing.Color.Black);
            })
            //
            .MainTableTemplate(template =>
            {
                template.BasicTemplate(BasicTemplate.ProfessionalTemplate);
            })
            .MainTablePreferences(table =>
            {
                table.ColumnsWidthsType(TableColumnWidthType.Relative);
                table.GroupsPreferences(new GroupsPreferences
                {
                    GroupType = GroupType.HideGroupingColumns,
                    RepeatHeaderRowPerGroup = true,
                    ShowOneGroupPerPage = true,
                    SpacingBeforeAllGroupsSummary = 5f,
                    NewGroupAvailableSpacingThreshold = 150,
                    SpacingAfterAllGroupsSummary = 5f
                });
                table.SpacingAfter(4f);
            });

            return pdf;
        }
    }
}
