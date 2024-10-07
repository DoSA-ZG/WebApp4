using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje projektima.
    /// </summary>
    public class ProjektController : Controller
    {

        private readonly Rppp04Context ctx;
        private readonly ILogger<ProjektController> logger;
        private readonly AppSettings appData;

        /// <summary>
        /// Konstruktor za ProjektController.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Opcije aplikacije koje se tiču straničenja.</param>
        /// <param name="logger">Logger za zapisivanje događaja.</param>
        public ProjektController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjektController> logger)
        {
            this.ctx = ctx;
            this.appData = options.Value;
            this.logger = logger;
        }

        /// <summary>
        /// Prikazuje listu projekata s opcijama za straničenje i sortiranje.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Prikazuje listu projekata.</returns>
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;
            var query = from p in ctx.Projekt
                        join vp in ctx.VrstaProjekta on p.VrstaProjektaId equals vp.Id
                        join n in ctx.Narucitelj on p.NaruciteljId equals n.Id
                        join d in ctx.Dokument on p.Id equals d.ProjektId into dokumenti
                        select new ViewProjektInfo
                        {
                            IdProjekt = p.Id,
                            NazivProjekt = p.NazivProjekt,
                            VrstaProjektId = p.VrstaProjektaId,
                            Vrsta = vp.Vrsta,
                            DatumIsporukaPr = p.DatumIsporukaPr,
                            NaruciteljId = p.NaruciteljId,
                            NazivNarucitelj = n.NazivNarucitelj,
                            Dokumenti = dokumenti.ToList()
                        };

            int count = await query.CountAsync();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if (count > 0 && (page < 1 || page > pagingInfo.TotalPages))
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var projekti = await query
                                .Skip((page - 1) * pagesize)
                                .Take(pagesize)
                                .ToListAsync();

            for (int i = 0; i < projekti.Count; i++)
            {
                projekti[i].Position = (page - 1) * pagesize + i;
            }

            var viewModel = new ProjektiViewModel
            {
                Projekti = projekti,
                PagingInfo = pagingInfo
            };

            return View(viewModel);
        }

        /// <summary>
        /// Prikazuje formu za dodavanje novog projekta.
        /// </summary>
        /// <returns>Prikazuje formu za unos novog projekta.</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            var projekt = new ProjektViewModel
            {
                DatumIsporukaPr = TrimDate(DateTime.Now)
            };
            return View(projekt);
        }

        /// <summary>
        /// Dodaje novi projekt u bazu podataka.
        /// </summary>
        /// <param name="model">Podaci o projektu.</param>
        /// <returns>Preusmjerava na akciju Edit nakon dodavanja projekta.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjektViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        Projekt pr = new Projekt
                        {
                            NazivProjekt = model.NazivProjekt,
                            KraticaProjekt = model.KraticaProjekt,
                            VrstaProjektaId = model.VrstaProjektaId.Value,
                            NaruciteljId = model.NaruciteljId.Value,
                            DatumIsporukaPr = model.DatumIsporukaPr
                        };

                        ctx.Add(pr);
                        await ctx.SaveChangesAsync();

                        List<Dokument> novaLista = new List<Dokument>();
                        foreach (var dwm in model.Dokumenti)
                        {
                            Dokument dok = new Dokument
                            {
                                NazivDok = dwm.NazivDok,
                                VrstaDokumentaId = dwm.VrstaDokumentaId,
                                StatusDokumentaId = dwm.StatusDokumentaId,
                                EkstenzijaDokumenta = dwm.EkstenzijaDokumenta,
                                DatumZadIzmj = dwm.DatumZadIzmj,
                                VrPrijenos = dwm.VrPrijenos,
                                ProjektId = pr.Id
                            };

                            if (dwm.DatotekaFile != null && dwm.DatotekaFile.Length > 0)
                            {
                                var fileName = Path.GetFileName(dwm.DatotekaFile.FileName);
                                var fileExtension = Path.GetExtension(fileName);
                                var newFileName = String.Concat(Convert.ToString(Guid.NewGuid()), fileExtension);

                                using (var target = new MemoryStream())
                                {
                                    dwm.DatotekaFile.CopyTo(target);
                                    dok.Datoteka = target.ToArray();
                                }
                            }

                            novaLista.Add(dok);
                        }

                        ctx.AddRange(novaLista);
                        await ctx.SaveChangesAsync();

                        transaction.Commit();

                        logger.LogInformation($"Projekt s ID: {pr.Id} uspješno dodan.");
                        TempData[Constants.Message] = $"Projekt s ID: {pr.Id} uspješno dodan.";
                        TempData[Constants.ErrorOccurred] = false;

                        return RedirectToAction(nameof(Edit), new { id = pr.Id });
                    }
                    catch (Exception exc)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        await PrepareDropDownLists();
                        return View(model);
                    }
                }
            }
            else
            {
                await PrepareDropDownLists();
                return View(model);
            }
        }


        /// <summary>
        /// Prikazuje formu za uređivanje projekta.
        /// </summary>
        /// <param name="id">ID projekta koji se uređuje.</param>
        /// <param name="position">Pozicija na stranici.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Prikazuje formu za uređivanje projekta.</returns>
        [HttpGet]
        public Task<IActionResult> Edit(int id, int? position, int page = 1, int sort = 1, bool ascending = true)
        {
            return Show(id, position, page, sort, ascending, viewName: nameof(Edit));
        }

        /// <summary>
        /// Sprema uređene informacije o projektu u bazu podataka.
        /// </summary>
        /// <param name="model">Podaci o projektu.</param>
        /// <param name="position">Pozicija na stranici.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Preusmjerava na akciju Edit nakon spremanja promjena.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjektViewModel model, int? position, int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            ViewBag.Position = position;

            if (ModelState.IsValid)
            {
                var projekt = await ctx.Projekt
                    .Include(d => d.Dokument)
                    .Where(d => d.Id == model.IdProjekt)
                    .FirstOrDefaultAsync();

                if (projekt == null)
                {
                    return NotFound("Ne postoji projekt s ID: " + model.IdProjekt + ".");
                }

                projekt.NazivProjekt = model.NazivProjekt;
                projekt.KraticaProjekt = model.KraticaProjekt;
                projekt.DatumIsporukaPr = model.DatumIsporukaPr;
                projekt.NaruciteljId = model.NaruciteljId;
                projekt.VrstaProjektaId = model.VrstaProjektaId;

                List<int> idDokumenata = model.Dokumenti
                                          .Where(s => s.Id > 0)
                                          .Select(s => s.Id)
                                          .ToList();

                ctx.RemoveRange(projekt.Dokument.Where(s => !idDokumenata.Contains(s.Id)));

                foreach (var dokument in model.Dokumenti)
                {
                    var noviDokument = projekt.Dokument.FirstOrDefault(s => s.Id == dokument.Id);

                    if (noviDokument != null)
                    {
                        if (dokument.VrstaDokumentaId != null)
                        {
                            noviDokument.VrstaDokumentaId = dokument.VrstaDokumentaId;
                        }

                        if (dokument.StatusDokumentaId != null)
                        {
                            noviDokument.StatusDokumentaId = dokument.StatusDokumentaId;
                        }

                        noviDokument.Id = dokument.Id;
                        noviDokument.NazivDok = dokument.NazivDok;
                        noviDokument.EkstenzijaDokumenta = dokument.EkstenzijaDokumenta;
                        noviDokument.VrPrijenos = dokument.VrPrijenos;
                        noviDokument.DatumZadIzmj = dokument.DatumZadIzmj;

                        if (dokument.DatotekaFile != null && dokument.DatotekaFile.Length > 0)
                        {
                            using (var target = new MemoryStream())
                            {
                                dokument.DatotekaFile.CopyTo(target);
                                noviDokument.Datoteka = target.ToArray();
                            }
                        }
                    }
                }

                if (model.NoviDokumenti != null)
                {

                    foreach (var dokument in model.NoviDokumenti)
                    {
                        Dokument noviDokument = new Dokument();
                        noviDokument.Id = dokument.Id;
                        noviDokument.NazivDok = dokument.NazivDok;

                        if (dokument.VrstaDokumentaId != null && dokument.StatusDokumentaId != null)
                        {
                            noviDokument.VrstaDokumentaId = dokument.VrstaDokumentaId;
                            noviDokument.StatusDokumentaId = dokument.StatusDokumentaId;
                        }

                        noviDokument.ProjektId = projekt.Id;
                        noviDokument.EkstenzijaDokumenta = dokument.EkstenzijaDokumenta;
                        noviDokument.VrPrijenos = dokument.VrPrijenos;
                        noviDokument.DatumZadIzmj = dokument.DatumZadIzmj;

                        if (dokument.DatotekaFile != null && dokument.DatotekaFile.Length > 0)
                        {
                            using (var target = new MemoryStream())
                            {
                                dokument.DatotekaFile.CopyTo(target);
                                noviDokument.Datoteka = target.ToArray();
                            }
                        }

                        projekt.Dokument.Add(noviDokument);
                    }
                }

                try
                {
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Projekt s ID: {projekt.Id} uspješno ažuriran.");
                    TempData[Constants.Message] = $"Projekt s ID: {projekt.Id} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Edit), new
                    {
                        id = projekt.Id,
                        position,
                        page,
                        sort,
                        ascending
                    });

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        /// <summary>
        /// Priprema padajuće liste za formu za dodavanje i uređivanje projekata.
        /// </summary>
        /// <returns>Popunjava ViewBag s padajućim listama za formu.</returns>
        public async Task PrepareDropDownLists()
        {
            var vrsteProj = await ctx.VrstaProjekta
                            .OrderBy(p => p.Vrsta)
                            .Select(p => new { Id = p.Id, Value = p.Vrsta })
                            .ToListAsync();

            ViewBag.VrsteProj = new SelectList(vrsteProj, "Id", "Value");

            var narucitelji = await ctx.Narucitelj
                              .OrderBy(n => n.NazivNarucitelj)
                              .Select(n => new { Id = n.Id, Value = n.NazivNarucitelj })
                              .ToListAsync();

            ViewBag.Narucitelji = new SelectList(narucitelji, "Id", "Value");

            var statusiDok = await ctx.StatusDokumenta
                                   .OrderBy(s => s.StatusDok)
                                   .Select(s => new { Id = s.Id, Value = s.StatusDok })
                                   .ToListAsync();

            ViewBag.Statusi = new SelectList(statusiDok, "Id", "Value");

            var vrsteDok = await ctx.VrstaDokumenta
                                   .OrderBy(s => s.VrstaDok)
                                   .Select(s => new { Id = s.Id, Value = s.VrstaDok })
                                   .ToListAsync();

            ViewBag.VrsteDok = new SelectList(vrsteDok, "Id", "Value");
        }

        /// <summary>
        /// Prilagođava datum i vrijeme na način da postavlja sekunde na nulu.
        /// </summary>
        /// <param name="dateTime">Ulazni datum i vrijeme.</param>
        /// <returns>Datum i vrijeme sa sekundama postavljenim na nulu.</returns>
        private DateTime TrimDate(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
        }

        /// <summary>
        /// Prikazuje detalje o određenom projektu.
        /// </summary>
        /// <param name="id">ID projekta čiji se detalji prikazuju.</param>
        /// <param name="position">Pozicija na stranici.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <param name="viewName">Ime pogleda za prikaz.</param>
        /// <returns>Prikazuje detalje o projektu.</returns>
        public async Task<IActionResult> Show(int id, int? position, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Show))
        {
            await PrepareDropDownLists();
            var projekt = await ctx.Projekt
                                    .Where(d => d.Id == id)
                                    .Select(d => new ProjektViewModel
                                    {
                                        IdProjekt = d.Id,
                                        NazivProjekt = d.NazivProjekt,
                                        KraticaProjekt = d.KraticaProjekt,
                                        DatumIsporukaPr = d.DatumIsporukaPr,
                                        NaruciteljId = d.NaruciteljId,
                                        VrstaProjektaId = d.VrstaProjektaId
                                    })
                                    .FirstOrDefaultAsync();
            if (projekt == null)
            {
                return NotFound($"Projekt s ID: {id} ne postoji.");
            }
            else
            {


                projekt.NazivNarucitelj = await (from p in ctx.Projekt
                                                 join n in ctx.Narucitelj on p.NaruciteljId equals n.Id
                                                 where p.Id == id
                                                 select n.NazivNarucitelj).FirstOrDefaultAsync();

                projekt.Vrsta = await (from p in ctx.Projekt
                                                 join n in ctx.VrstaProjekta on p.VrstaProjektaId equals n.Id
                                                 where p.Id == id
                                                 select n.Vrsta).FirstOrDefaultAsync();


                var dokumenti = await ctx.Dokument
                                      .Where(s => s.ProjektId == projekt.IdProjekt)
                                      .OrderBy(s => s.ProjektId)
                                      .Select(s => new DokumentViewModel
                                      {
                                          Id = s.Id,
                                          NazivDok = s.NazivDok,
                                          VrstaDokumenta = s.IdVrstaDokNavigation.VrstaDok,
                                          StatusDokumenta = s.IdStatusDokNavigation.StatusDok,
                                          EkstenzijaDokumenta = s.EkstenzijaDokumenta,
                                          VrPrijenos = s.VrPrijenos,
                                          ProjektId = s.IdProjektNavigation.Id,
                                          DatumZadIzmj = s.DatumZadIzmj
                                      })
                                      .ToListAsync();

                projekt.Dokumenti = dokumenti;

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Position = position;

                await PrepareDropDownLists();
                return View(viewName, projekt);
            }
        }

        /// <summary>
        /// Briše određeni projekt iz baze podataka.
        /// </summary>
        /// <param name="id">ID projekta koji se briše.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Preusmjerava na akciju Index nakon brisanja projekta.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var projekt = await ctx.Projekt
                                    .Where(d => d.Id == id)
                                    .SingleOrDefaultAsync();
            if (projekt != null)
            {
                try
                {

                    var dokumentiZaObrisati = await ctx.Dokument
                                         .Where(d => d.ProjektId == id)
                                         .ToListAsync();

                    foreach (var dok in dokumentiZaObrisati)
                    {
                        if (dok != null)
                        {
                            ctx.Remove(dok);
                            await ctx.SaveChangesAsync();
                        }
                    }


                    ctx.Remove(projekt);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Projekt s ID: {projekt.Id} uspješno obrisan.");
                    TempData[Constants.Message] = $"Projekt s ID: {projekt.Id} uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = $"Pogreška prilikom brisanja projekta s ID: {projekt.Id}" + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
            }
            else
            {
                TempData[Constants.Message] = "Ne postoji projekt s ID: " + id + ".";
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }
    }
}