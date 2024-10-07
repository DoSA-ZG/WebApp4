using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NHibernate.Cache;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Net.Mime;
using System.Text.Json;

namespace RPPP_WebApp.Controllers
{

    /// <summary>
    /// Kontroler za upravljanje dokumentima
    /// </summary>
    public class DokumentController : Controller
    {

        private readonly Rppp04Context ctx;
        private readonly ILogger<DokumentController> logger;
        private readonly AppSettings appData;

        /// <summary>
        /// Konstruktor za DokumentController.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Dodatne opcije koje se tiču straničenja.</param>
        /// <param name="logger">Logger za zapisivanje događaja.</param>
        public DokumentController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<DokumentController> logger)
        {
            this.ctx = ctx;
            this.appData = options.Value;
            this.logger = logger;
        }

        /// <summary>
        /// Prikazuje listu dokumenata s opcijama za straničenje i sortiranje.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Prikazuje listu dokumenata.</returns>
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.Dokument.AsNoTracking();
            int count = await query.CountAsync();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var dokumenti = await query
                          .Select(d => new DokumentViewModel
                          {
                              Id = d.Id,
                              NazivDok = d.NazivDok,
                              StatusDokumenta = d.IdStatusDokNavigation.StatusDok,
                              VrstaDokumenta = d.IdVrstaDokNavigation.VrstaDok,
                              EkstenzijaDokumenta = d.EkstenzijaDokumenta,
                              VrPrijenos = d.VrPrijenos,
                              DatumZadIzmj = d.DatumZadIzmj,
                              NazivProjekt = d.IdProjektNavigation.NazivProjekt,
                              Datoteka = d.Datoteka
                          })
                          .Skip((page - 1) * pagesize)
                          .Take(pagesize)
                          .ToListAsync();


            var model = new DokumentiViewModel
            {
                Dokumenti = dokumenti,
                PagingInfo = pagingInfo
            };


            return View(model);
        }

        /// <summary>
        /// Prikazuje formu za dodavanje novog dokumenta.
        /// </summary>
        /// <returns>Prikazuje formu za unos novog dokumenta.</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }

        /// <summary>
        /// Dodaje novi dokument u bazu podataka.
        /// </summary>
        /// <param name="dokument">Podaci o dokumentu.</param>
        /// <param name="datoteka">Datoteka dokumenta.</param>
        /// <returns>Preusmjerava na akciju Index nakon dodavanja dokumenta.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Dokument dokument, IFormFile datoteka)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    if (datoteka != null && datoteka.Length > 0)
                    {

                        var fileName = Path.GetFileName(datoteka.FileName.Split(".")[0]);
                        var fileExtension = Path.GetExtension(datoteka.FileName);
                        var newFileName = String.Concat(Convert.ToString(Guid.NewGuid()), fileExtension);

                        using (var target = new MemoryStream())
                        {
                            datoteka.CopyTo(target);
                            dokument.Datoteka = target.ToArray();
                        }

                    }
                    ctx.Add(dokument);
                    ctx.SaveChanges();

                    logger.LogInformation($"Dokument  {dokument.NazivDok} dodan.");
                    TempData[Constants.Message] = $"Dokument  {dokument.NazivDok} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(dokument);
                }
            }
            else
            {
                return View(dokument);
            }
        }

        /// <summary>
        /// Preuzima sadržaj dokumenta s određenim ID-om.
        /// </summary>
        /// <param name="id">ID dokumenta koji se preuzima.</param>
        /// <returns>Vraća sadržaj dokumenta.</returns>
        public IActionResult GetDocumentContent(int id)
        {
            try
            {
                var dokument = ctx.Dokument.Find(id);

                if (dokument == null)
                {
                    return NotFound();
                }

                var ctype = GetContentType(dokument.EkstenzijaDokumenta);

                var contentDisposition = new ContentDisposition
                {
                    FileName = System.Net.WebUtility.UrlEncode(dokument.NazivDok) + dokument.EkstenzijaDokumenta,
                    Inline = false
                };

                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                return File(dokument.Datoteka, ctype);
            }
            catch (Exception exc)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exc.Message);
            }
        }

        /// <summary>
        /// Pomoćna metoda za određivanje tipa sadržaja (Content Type) datoteke na temelju njezine ekstenzije.
        /// </summary>
        /// <param name="extension">Ekstenzija datoteke.</param>
        /// <returns>Tipo sadržaja (Content Type) za datoteku.</returns>
        private string GetContentType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".pdf":
                    return "application/pdf";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".txt":
                    return "text/plain";
                default:
                    return "application/octet-stream";
            }
        }

        /// <summary>
        /// Prikazuje formu za uređivanje postojećeg dokumenta.
        /// </summary>
        /// <param name="id">ID dokumenta koji se uređuje.</param>
        /// <returns>Prikazuje formu za uređivanje dokumenta.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dokument = await ctx.Dokument
                                  .AsNoTracking()
                                  .Where(d => d.Id == id)
                                  .SingleOrDefaultAsync();
            if (dokument != null)
            {
                await PrepareDropDownLists();
                return PartialView(dokument);
            }
            else
            {
                return NotFound($"Neispravan ID dokumenta: {id}");
            }
        }

        /// <summary>
        /// Uređuje postojeći dokument u bazi podataka.
        /// </summary>
        /// <param name="dokument">Podaci o dokumentu.</param>
        /// <param name="datoteka">Datoteka dokumenta.</param>
        /// <returns>Preusmjerava na akciju Get nakon uređivanja dokumenta.</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(Dokument dokument, IFormFile datoteka)
        {
            if (dokument == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            Dokument dbdoc = await ctx.Dokument.FindAsync(dokument.Id);
            if (dbdoc == null)
            {
                return NotFound($"Neispravan ID dokumenta: {dokument?.Id}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    dbdoc.NazivDok = dokument.NazivDok;
                    dbdoc.VrstaDokumentaId = dokument.VrstaDokumentaId;
                    dbdoc.StatusDokumentaId = dokument.StatusDokumentaId;
                    dbdoc.EkstenzijaDokumenta = dokument.EkstenzijaDokumenta;
                    dbdoc.ProjektId = dokument.ProjektId;
                    dbdoc.VrPrijenos = dokument.VrPrijenos;
                    dbdoc.DatumZadIzmj = dokument.DatumZadIzmj;

                    if (datoteka != null && datoteka.Length > 0)
                    {
                        using (var target = new MemoryStream())
                        {
                            datoteka.CopyTo(target);
                            dbdoc.Datoteka = target.ToArray();
                        }
                    }

                    await ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Get), new { id = dokument.Id });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownLists();
                    return PartialView(dokument);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return PartialView(dokument);
            }
        }

        /// <summary>
        /// Prikazuje detalje o određenom dokumentu.
        /// </summary>
        /// <param name="id">ID dokumenta čiji se detalji prikazuju.</param>
        /// <returns>Prikazuje detalje o dokumentu.</returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var dokument = await ctx.Dokument
                                  .Where(d => d.Id == id)
                                  .Select(d => new DokumentViewModel
                                  {
                                      Id = d.Id,
                                      NazivDok = d.NazivDok,
                                      StatusDokumenta = d.IdStatusDokNavigation.StatusDok,
                                      VrstaDokumenta = d.IdVrstaDokNavigation.VrstaDok,
                                      EkstenzijaDokumenta = d.EkstenzijaDokumenta,
                                      VrPrijenos = d.VrPrijenos,
                                      DatumZadIzmj = d.DatumZadIzmj,
                                      ProjektId = d.ProjektId,
                                      NazivProjekt = d.IdProjektNavigation.NazivProjekt,
                                      Datoteka = d.Datoteka
                                  })
                                  .SingleOrDefaultAsync();
            if (dokument != null)
            {
                return PartialView(dokument);
            }
            else
            {
                return NotFound($"Neispravan ID dokumenta: {id}");
            }
        }


        /// <summary>
        /// Briše dokument s određenim ID-om iz baze podataka.
        /// </summary>
        /// <param name="id">ID dokumenta koji se briše.</param>
        /// <returns>Rezultat brisanja dokumenta.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var dokument = await ctx.Dokument.FindAsync(id);
            if (dokument != null)
            {
                try
                {
                    string naziv = dokument.NazivDok;
                    ctx.Remove(dokument);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Dokument {naziv} sa ID: {id} uspješno obrisan.");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Dokument {naziv} sa ID: {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja dokumenta: {exc.CompleteExceptionMessage()}.");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Dokument sa ID: {id} ne postoji.");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return responseMessage.MessageType == MessageType.Success ? new EmptyResult() : await Get(id);
        }

        /// <summary>
        /// Priprema padajuće liste za formu za dodavanje i uređivanje dokumenata.
        /// </summary>
        /// <returns>Popunjava ViewBag s padajućim listama za formu.</returns>
        private async Task PrepareDropDownLists()
        {
            var dov = await ctx.StatusDokumenta
                              .Where(d => d.StatusDok == "Dovršen")
                              .Select(d => new { d.StatusDok, d.Id })
                              .FirstOrDefaultAsync();
            var statusiDok = await ctx.StatusDokumenta
                                  .Where(d => d.StatusDok != "Dovršen")
                                  .OrderBy(d => d.StatusDok)
                                  .Select(d => new { d.StatusDok, d.Id })
                                  .ToListAsync();
            if (dov != null)
            {
                statusiDok.Insert(0, dov);
            }
            ViewBag.statusiDok = new SelectList(statusiDok, nameof(dov.Id), nameof(dov.StatusDok));

            var vrd = await ctx.VrstaDokumenta
                              .Where(d => d.VrstaDok == "Plan")
                              .Select(d => new { d.VrstaDok, d.Id })
                              .FirstOrDefaultAsync();
            var vrsteDok = await ctx.VrstaDokumenta
                                  .Where(d => d.VrstaDok != "Plan")
                                  .OrderBy(d => d.VrstaDok)
                                  .Select(d => new { d.VrstaDok, d.Id })
                                  .ToListAsync();
            if (vrd != null)
            {
                vrsteDok.Insert(0, vrd);
            }
            ViewBag.vrsteDok = new SelectList(vrsteDok, nameof(vrd.Id), nameof(vrd.VrstaDok));

            var prd = await ctx.Projekt
                              .Where(d => d.NazivProjekt == "Hrvatske šume")
                              .Select(d => new { d.NazivProjekt, d.Id })
                              .FirstOrDefaultAsync();
            var projektir = await ctx.Projekt
                                  .OrderBy(d => d.NazivProjekt)
                                  .Select(d => new { d.NazivProjekt, d.Id })
                                  .ToListAsync();

            if (prd != null)
            {
                projektir.Insert(0, prd);
            }
            ViewBag.projekti = new SelectList(projektir, nameof(prd.Id), nameof(prd.NazivProjekt));
        }


    }
}