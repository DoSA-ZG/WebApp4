using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using OfficeOpenXml;
using PdfRpt.ColumnsItemsTemplates;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Model;
using RPPP_WebApp.ModelsPartial;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers;

/// <summary>
/// Klasa koja predstavlja upravljač koji upravlja stvaranjem izvješća
/// </summary>
public class SuradnikReportController : Controller
{
    private readonly Rppp04Context ctx;
    private readonly IWebHostEnvironment environment;
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>
    /// Konstruktor za upravljač izvještajima suradnika
    /// </summary>
    /// <param name="ctx">kontekst za spajanje na bazu</param>
    /// <param name="environment">okruženje</param>
    public SuradnikReportController(Rppp04Context ctx, IWebHostEnvironment environment)
    {
        this.ctx = ctx;
        this.environment = environment;
    }

    /// <summary>
    /// Funkcija za izradu excel tablice sa svim suradnicima iz baze podataka i njihovim glavnim atributima
    /// </summary>
    /// <returns>Excel datoteka s popisom suradnika</returns>
    public async Task<IActionResult> SuradniciExcel()
    {
        var suradnici = await ctx.Suradnik
            .Include(s => s.KorisnickiRacun)
            .Include(s => s.VrstaSuradnika)
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();
        byte[] content;
        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis suradnika";
            excel.Workbook.Properties.Author = "FER-ZPR";
            var worksheet = excel.Workbook.Worksheets.Add("Suradnici");

            worksheet.Cells[1, 1].Value = "Ime";
            worksheet.Cells[1, 2].Value = "Prezime";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "Organizacija";
            worksheet.Cells[1, 5].Value = "Broj telefona";
            worksheet.Cells[1, 6].Value = "Vrsta suradnika";
            worksheet.Cells[1, 7].Value = "Korisnički račun (ID)";

            for (int i = 0; i < suradnici.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = suradnici[i].Ime;
                worksheet.Cells[i + 2, 2].Value = suradnici[i].Prezime;
                worksheet.Cells[i + 2, 3].Value = suradnici[i].Email;
                worksheet.Cells[i + 2, 4].Value = suradnici[i].Organizacija;
                worksheet.Cells[i + 2, 5].Value = suradnici[i].BrojTelefona;
                worksheet.Cells[i + 2, 6].Value = suradnici[i].VrstaSuradnika.Vrsta;
                worksheet.Cells[i + 2, 7].Value = suradnici[i].KorisnickiRacunId;
            }

            worksheet.Cells[1, 1, suradnici.Count + 1, 7].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "suradnici.xlsx");
    }

    /// <summary>
    /// Funkcija za izradu excel tablice sa svim ulogama svih suradnika iz baze podataka i njihovim glavnim atributima
    /// </summary>
    /// <returns>Excel datoteka s popisom uloga suradnika</returns>
    public async Task<IActionResult> SuradnikUlogeExcel()
    {
        var suradnikUloge = await ctx.SuradnikUloga
            .Include(d => d.Suradnik)
            .Include(d => d.Projekt)
            .Include(d => d.VrstaUloge)
            .AsNoTracking()
            .OrderBy(d => d.SuradnikId)
            .ToListAsync();
        byte[] content;
        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis uloga suradnika";
            excel.Workbook.Properties.Author = "FER-ZPR";
            var worksheet = excel.Workbook.Worksheets.Add("Suradnik Uloge");

            worksheet.Cells[1, 1].Value = "Datum početka";
            worksheet.Cells[1, 2].Value = "Datum kraja";
            worksheet.Cells[1, 3].Value = "Suradnik";
            worksheet.Cells[1, 4].Value = "Projekt";
            worksheet.Cells[1, 5].Value = "Vrsta uloge";

            for (int i = 0; i < suradnikUloge.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Style.Numberformat.Format = "dd-mm-yyyy";
                worksheet.Cells[i + 2, 1].Value = suradnikUloge[i].DatumPocetak;
                worksheet.Cells[i + 2, 2].Style.Numberformat.Format = "dd-mm-yyyy";
                worksheet.Cells[i + 2, 2].Value = suradnikUloge[i].DatumKraj;
                worksheet.Cells[i + 2, 3].Value =
                    $"{suradnikUloge[i].Suradnik.Ime} {suradnikUloge[i].Suradnik.Prezime}";
                worksheet.Cells[i + 2, 4].Value = suradnikUloge[i].Projekt.NazivProjekt;
                worksheet.Cells[i + 2, 5].Value = suradnikUloge[i].VrstaUloge.Vrsta;
            }

            worksheet.Cells[1, 1, suradnikUloge.Count + 1, 5].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "suradnikuloge.xlsx");
    }


    /// <summary>
    /// Stvara pdf izvještaj svih suradnika s njihovim ulogama na projektima
    /// </summary>
    /// <returns>pdf datoteku svih suradnika s njihovim ulogama na projektima</returns>
    public async Task<IActionResult> Suradnici()
    {
        string naslov = $"Popis suradnika s ulogama na projektima";
        var ulogeSuradnika = await ctx.SuradnikUloga
            .Include(s => s.Suradnik)
            .Include(s => s.VrstaUloge)
            .Include(s => s.Projekt)
            .Include(s => s.Suradnik.VrstaSuradnika)
            .Include(s => s.Suradnik.KorisnickiRacun)
            .OrderBy(s => s.SuradnikId)
            .Select(s => new SuradnikUlogaDenorm
            {
                SuradnikId = s.SuradnikId,
                Organizacija = s.Suradnik.Organizacija,
                Email = s.Suradnik.Email,
                Ime = s.Suradnik.Ime,
                Prezime = s.Suradnik.Prezime,
                BrojTelefona = s.Suradnik.BrojTelefona,
                VrstaSuradnika = s.Suradnik.VrstaSuradnika.Vrsta,
                StupanjPrava = s.Suradnik.KorisnickiRacun.StupanjPrava,
                DatumPocetak = s.DatumPocetak,
                DatumKraj = s.DatumKraj,
                NazivProjekt = s.Projekt.NazivProjekt,
                VrstaUloge = s.VrstaUloge.Vrsta,
            })
            .ToListAsync();
        //Console.WriteLine(ulogeSuradnika.ToJson());
        ulogeSuradnika.ForEach(s => s.UrlSuradnika = Url.Action("Show", "SuradnikMD", new { id = s.SuradnikId }));
        PdfReport report = CreateReport(naslov);

        #region Podnožje i zaglavlje

        report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true); // It's a default setting to improve the performance.
                header.CustomHeader(new MasterDetailsHeaders(naslov)
                {
                    PdfRptFont = header.PdfFont
                });
            });

        #endregion

        #region Postavljanje izvora podataka i stupaca

        report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(ulogeSuradnika));


        report.MainTableColumns(columns =>
        {
            #region Stupci po kojima se grupira

            columns.AddColumn(column =>
            {
                column.PropertyName("SuradnikId");
                column.Group(
                    (val1, val2) => { return (int)val1 == (int)val2; });
            });

            #endregion

            columns.AddColumn(column =>
            {
                column.IsRowNumber(true);
                column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                column.IsVisible(true);
                column.Width(1);
                column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
            });
            columns.AddColumn(column =>
            {
                column.PropertyName<SuradnikUlogaDenorm>(x => x.DatumPocetak);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Width(2);
                column.HeaderCell("Datum početka:", horizontalAlignment: HorizontalAlignment.Center);
                column.ColumnItemsTemplate(template =>
                {
                    template.TextBlock();
                    template.DisplayFormatFormula(obj => ((DateTime)obj).ToString("dd.MM.yyyy"));
                });
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<SuradnikUlogaDenorm>(x => x.DatumKraj);
                column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                column.IsVisible(true);
                column.Width(2);
                column.HeaderCell("Datum kraja:", horizontalAlignment: HorizontalAlignment.Center);
                column.ColumnItemsTemplate(template =>
                {
                    template.TextBlock();
                    template.DisplayFormatFormula(obj => ((DateTime)obj).ToString("dd.MM.yyyy"));
                });
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<SuradnikUlogaDenorm>(x => x.NazivProjekt);
                column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                column.IsVisible(true);
                column.Width(3);
                column.HeaderCell("Projekt:", horizontalAlignment: HorizontalAlignment.Right);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<SuradnikUlogaDenorm>(x => x.VrstaUloge);
                column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                column.IsVisible(true);
                column.Width(3);
                column.HeaderCell("Vrsta uloge:", horizontalAlignment: HorizontalAlignment.Right);
            });
        });

        #endregion

        byte[] pdf = report.GenerateAsByteArray();

        if (pdf != null)
        {
            Response.Headers.Add("content-disposition", "inline; filename=suradnici.pdf");
            return File(pdf, "application/pdf");
        }
        else
            return NotFound();
    }

    #region Master-detail header

    /// <summary>
    /// klasa koja predstavlja zaglavlje unutar pdf izvještaja
    /// </summary>
    public class MasterDetailsHeaders : IPageHeader
    {
        private string naslov;

        public MasterDetailsHeaders(string naslov)
        {
            this.naslov = naslov;
        }

        public IPdfFont PdfRptFont { set; get; }

        /// <summary>
        /// stvara zaglavlje pdf dokumenta sa informacijama o suradniku
        /// </summary>
        /// <param name="pdfDoc">pdf dokument</param>
        /// <param name="pdfWriter"></param>
        /// <param name="newGroupInfo"></param>
        /// <param name="summaryData"></param>
        /// <returns>zaglavlje za pdf dokument</returns>
        public PdfGrid RenderingGroupHeader(Document pdfDoc, PdfWriter pdfWriter, IList<CellData> newGroupInfo,
            IList<SummaryCellData> summaryData)
        {
            // var suradnikId = newGroupInfo.GetSafeStringValueOf(nameof(SuradnikUlogaDenorm.SuradnikId));
            // var urlSuradnika = newGroupInfo.GetSafeStringValueOf(nameof(SuradnikUlogaDenorm.UrlSuradnika));
            var organizacija = newGroupInfo.GetSafeStringValueOf(nameof(SuradnikUlogaDenorm.Organizacija));
            // var datumPocetak = (DateTime)newGroupInfo.GetValueOf(nameof(SuradnikUlogaDenorm.DatumPocetak));
            // var datumKraj = (DateTime)newGroupInfo.GetValueOf(nameof(SuradnikUlogaDenorm.DatumKraj));
            var email = newGroupInfo.GetSafeStringValueOf(nameof(SuradnikUlogaDenorm.Email));
            var ime = newGroupInfo.GetSafeStringValueOf(nameof(SuradnikUlogaDenorm.Ime));
            var prezime = newGroupInfo.GetSafeStringValueOf(nameof(SuradnikUlogaDenorm.Prezime));
            var brojTelefona = newGroupInfo.GetSafeStringValueOf(nameof(SuradnikUlogaDenorm.BrojTelefona));
            var vrstaSuradnika = newGroupInfo.GetSafeStringValueOf(nameof(SuradnikUlogaDenorm.VrstaSuradnika));
            var stupanjPrava = newGroupInfo.GetSafeStringValueOf(nameof(SuradnikUlogaDenorm.StupanjPrava));

            var table = new PdfGrid(relativeWidths: new[] { 2f, 5f, 2f, 3f }) { WidthPercentage = 100 };

            table.AddSimpleRow(
                (cellData, cellProperties) =>
                {
                    cellData.Value = "Id suradnika:";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.TableRowData = newGroupInfo; //postavi podatke retka za ćeliju
                    var cellTemplate = new HyperlinkField(BaseColor.Black, false)
                    {
                        TextPropertyName = nameof(SuradnikUlogaDenorm.SuradnikId),
                        NavigationUrlPropertyName = nameof(SuradnikUlogaDenorm.UrlSuradnika),
                        BasicProperties = new CellBasicProperties
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            PdfFontStyle = DocumentFontStyle.Bold,
                            PdfFont = PdfRptFont
                        }
                    };

                    cellData.CellTemplate = cellTemplate;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = "Organizacija:";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = organizacija;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                });

            table.AddSimpleRow(
                (cellData, cellProperties) =>
                {
                    cellData.Value = "E-mail:";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = email;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = "Broj telefona:";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = brojTelefona;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                });
            table.AddSimpleRow(
                (cellData, cellProperties) =>
                {
                    cellData.Value = "Ime:";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = ime;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = "Prezime:";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = prezime;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                });
            table.AddSimpleRow(
                (cellData, cellProperties) =>
                {
                    cellData.Value = "Vrsta suradnika:";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = vrstaSuradnika;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = "Stupanj prava:";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = stupanjPrava;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                });


            return table.AddBorderToTable(borderColor: BaseColor.LightGray, spacingBefore: 5f);
        }

        /// <summary>
        /// renderira header u pdf dokumentu
        /// </summary>
        /// <param name="pdfDoc">pdf dokument</param>
        /// <param name="pdfWriter"></param>
        /// <param name="summaryData"></param>
        /// <returns></returns>
        public PdfGrid RenderingReportHeader(Document pdfDoc, PdfWriter pdfWriter, IList<SummaryCellData> summaryData)
        {
            var table = new PdfGrid(numColumns: 1) { WidthPercentage = 100 };
            table.AddSimpleRow(
                (cellData, cellProperties) =>
                {
                    cellData.Value = naslov;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Center;
                });
            return table.AddBorderToTable();
        }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Stvara i podešava pdf izvještaj
    /// </summary>
    /// <param name="naslov">naslov dokumenta</param>
    /// <returns></returns>
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
                    Application = "Rppp04.MVC Core",
                    Title = naslov
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })
            //fix za linux https://github.com/VahidN/PdfReport.Core/issues/40
            .DefaultFonts(fonts =>
            {
                fonts.Path(Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"),
                    Path.Combine(environment.WebRootPath, "fonts", "tahoma.ttf"));
                fonts.Size(9);
                fonts.Color(System.Drawing.Color.Black);
            })
            //
            .MainTableTemplate(template => { template.BasicTemplate(BasicTemplate.ProfessionalTemplate); })
            .MainTablePreferences(table =>
            {
                table.ColumnsWidthsType(TableColumnWidthType.Relative);
                //table.NumberOfDataRowsPerPage(20);
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

    #endregion
}