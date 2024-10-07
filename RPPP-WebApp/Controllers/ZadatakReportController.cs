using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RPPP_WebApp.Model;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.ModelsPartial;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PdfRpt.Core.Helper;
using PdfRpt.ColumnsItemsTemplates;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using RPPP_WebApp.Extensions;


namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Klasa koja predstavlja upravljač koji upravlja stvaranjem izvješća
    /// </summary>
    public class ZadatakReportController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        /// <summary>
        /// Konstruktor za upravljač izvještajima zadataka
        /// </summary>
        /// <param name="ctx">kontekst za spajanje na bazu</param>
        /// <param name="environment">okruženje</param>
        public ZadatakReportController(Rppp04Context ctx, IWebHostEnvironment environment)
        {
            this.ctx = ctx;
            this.environment = environment;
        }
        #region excelbezrefl
        /// <summary>
        /// Stvara izvještaj svih zadataka u xlsx formatu
        /// </summary>
        /// <returns>datoteku zadataka</returns>
        public async Task<IActionResult> ZadatciExcel()
        {
            var zadatci = await ctx.Zadatak
                                  .AsNoTracking()
                                  .OrderBy(d => d.OpisZadatak)
                                  .Select(d => new ZadatakViewModel
                                  {
                                      Id = d.Id,
                                      OpisZadatak = d.OpisZadatak,
                                      PlanPocetak = d.PlanPocetak,
                                      PlanKraj = d.PlanKraj,
                                      StvarniPocetak = d.StvarniPocetak,
                                      StvarniKraj = d.StvarniKraj,
                                      Email = d.Suradnik.Email,
                                      NazivPrioriteta = d.PrioritetZadatka.NazivPrioriteta,
                                      Status = d.StatusZadatka.Status
                                  })
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis zadataka";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Zadatci");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Opis zadatka";
                worksheet.Cells[1, 2].Value = "Planirani početak";
                worksheet.Cells[1, 3].Value = "Planirani kraj";
                worksheet.Cells[1, 4].Value = "Stvarni početak";
                worksheet.Cells[1, 5].Value = "Stvarni kraj";
                worksheet.Cells[1, 6].Value = "Suradnik";
                worksheet.Cells[1, 7].Value = "Prioritet";
                worksheet.Cells[1, 8].Value = "Status";




                for (int i = 0; i < zadatci.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = zadatci[i].OpisZadatak;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


                    worksheet.Cells[i + 2, 2].Value = zadatci[i].PlanPocetak;
                    worksheet.Cells[i + 2, 2].Style.Numberformat.Format = "dd-mm-yyyy";

                    worksheet.Cells[i + 2, 3].Value = zadatci[i].PlanKraj;
                    worksheet.Cells[i + 2, 3].Style.Numberformat.Format = "dd-mm-yyyy";

                    worksheet.Cells[i + 2, 4].Value = zadatci[i].StvarniPocetak;
                    worksheet.Cells[i + 2, 4].Style.Numberformat.Format = "dd-mm-yyyy";


                    worksheet.Cells[i + 2, 5].Value = zadatci[i].StvarniKraj;
                    worksheet.Cells[i + 2, 5].Style.Numberformat.Format = "dd-mm-yyyy";


                    worksheet.Cells[i + 2, 6].Value = zadatci[i].Email;
                    worksheet.Cells[i + 2, 7].Value = zadatci[i].NazivPrioriteta;
                    worksheet.Cells[i + 2, 8].Value = zadatci[i].Status;
                }

                worksheet.Cells[1, 1, zadatci.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "zadatci.xlsx");
        }
        #endregion

        /// <summary>
        /// Stvara izvještaj svih evidencija u xlsx formatu
        /// </summary>
        /// <returns>datoteku evidencija</returns>
        public async Task<IActionResult> EvidencijeExcel()
        {
            var evidencije = await ctx.EvidencijaRada
                                      .OrderByDescending(d => d.BrojSati)
                                      .Take(10)
                                      .Select(m => new EvidencijaRadaExcel
                                      {
                                          Id = m.Id,
                                          BrojSati = m.BrojSati,
                                          OpisRada = m.OpisRada,
                                          OpisZadatak = m.Zadatak.OpisZadatak,
                                          VrstaRada = m.VrstaRada.VrstaRada1,
                                          Email = m.Suradnik.Email,

                                      }
                   )
                                      .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = evidencije.CreateExcel("Dokumenti"))
            {
                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "evidencije.xlsx");
        }


        /// <summary>
        /// Stvara pdf izvještaj svih zadataka s evidencijama
        /// </summary>
        /// <returns>pdf datoteku zadataka sa evidencijama</returns>
        public async Task<IActionResult> Zadatci()
        {
     
            string naslov = $"Popis zadataka sa evidencijama";
            var evidencije = await ctx.EvidencijaRada
                                  .OrderBy(s => s.Id)
                                  .ThenBy(s => s.OpisRada)
                                  .Select(s => new EvidencijaDenorm
                                  {
                                      ZadatakId = (int)s.ZadatakId,
                                      BrojSati = s.BrojSati,
                                      OpisRada = s.OpisRada,
                                      OpisZadatak = s.Zadatak.OpisZadatak,
                                      VrstaRada = s.VrstaRada.VrstaRada1,
                                      EmailNositelj = s.Zadatak.Suradnik.Email,
                                      EmailRad = s.Suradnik.Email,
                                      Status = s.Zadatak.StatusZadatka.Status,
                                      PlanKraj = s.Zadatak.PlanKraj,
                                      PlanPocetak = s.Zadatak.PlanPocetak,

                                  })
                                  .ToListAsync();
            evidencije.ForEach(s => s.UrlZadatka = Url.Action("Change", "Zadatak", new { id = s.ZadatakId }));
            PdfReport report = CreateReport(naslov);
            #region Podnožje i zaglavlje
            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(evidencije));


            report.MainTableColumns(columns =>
            {
                #region Stupci po kojima se grupira
                columns.AddColumn(column =>
                {
                    column.PropertyName<EvidencijaRadaViewModel>(s => s.ZadatakId);
                    column.Group(
                        (val1, val2) =>
                        {
                            return (int)val1 == (int)val2;
                        });
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
                    column.PropertyName<EvidencijaDenorm>(x => x.OpisRada);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Width(4);
                    column.HeaderCell("Opis rada", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<EvidencijaDenorm>(x => x.BrojSati);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Width(1);
                    column.HeaderCell("Broj sati", horizontalAlignment: HorizontalAlignment.Center);
                    column.ColumnItemsTemplate(template =>
                    {
                        template.TextBlock();
                        template.DisplayFormatFormula(obj => obj == null || string.IsNullOrEmpty(obj.ToString())
                                                      ? string.Empty : obj.ToString());
                    });
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<EvidencijaDenorm>(x => x.VrstaRada);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Width(1);
                    column.HeaderCell("Vrsta rada", horizontalAlignment: HorizontalAlignment.Right);
                    
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<EvidencijaDenorm>(x => x.EmailRad);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Width(1);
                    column.HeaderCell("Email radnika", horizontalAlignment: HorizontalAlignment.Right);

                });

            });

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=zadatci.pdf");
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
            /// stvara zaglavlje pdf dokumenta sa informacijama o zadatku
            /// </summary>
            /// <param name="pdfDoc">pdf dokument</param>
            /// <param name="pdfWriter"></param>
            /// <param name="newGroupInfo"></param>
            /// <param name="summaryData"></param>
            /// <returns>zaglavlje za pdf dokument</returns>

            public PdfGrid RenderingGroupHeader(Document pdfDoc, PdfWriter pdfWriter, IList<CellData> newGroupInfo, IList<SummaryCellData> summaryData)
            {
                var idZadatak = newGroupInfo.GetSafeStringValueOf(nameof(EvidencijaDenorm.ZadatakId));
                var urlZadatka = newGroupInfo.GetSafeStringValueOf(nameof(EvidencijaDenorm.UrlZadatka));
                var emailNositelj = newGroupInfo.GetSafeStringValueOf(nameof(EvidencijaDenorm.EmailNositelj));
                var planPocetak = (DateTime)newGroupInfo.GetValueOf(nameof(EvidencijaDenorm.PlanPocetak));
                var planKraj = (DateTime)newGroupInfo.GetValueOf(nameof(EvidencijaDenorm.PlanKraj));
                var zadatakOpis = newGroupInfo.GetSafeStringValueOf(nameof(EvidencijaDenorm.OpisZadatak));
                var statusZad = newGroupInfo.GetSafeStringValueOf(nameof(EvidencijaDenorm.Status));

                var table = new PdfGrid(relativeWidths: new[] { 2f, 5f, 2f, 3f }) { WidthPercentage = 100 };

                table.AddSimpleRow(
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Id dokumenta:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.TableRowData = newGroupInfo; //postavi podatke retka za ćeliju
                        var cellTemplate = new HyperlinkField(BaseColor.Black, false)
                        {
                            TextPropertyName = nameof(EvidencijaDenorm.ZadatakId),
                            NavigationUrlPropertyName = nameof(EvidencijaDenorm.UrlZadatka),
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
                        cellData.Value = "Opis zadatka:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = zadatakOpis;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                        
                    });

                table.AddSimpleRow(
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Nositelj:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = emailNositelj;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Status zadatka";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = statusZad;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    });
                table.AddSimpleRow(
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Planirani pocetak:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = planPocetak;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.DisplayFormatFormula = obj => ((DateTime)obj).ToString("dd.MM.yyyy");
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Planirani kraj:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = planKraj;
                        cellProperties.DisplayFormatFormula = obj => ((DateTime)obj).ToString("dd.MM.yyyy");
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
                    Application = "Firma.MVC Core",
                    Title = naslov
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })
            //fix za linux https://github.com/VahidN/PdfReport.Core/issues/40
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


    }
}
#endregion