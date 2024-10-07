using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using NLog.LayoutRenderers;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection.Metadata;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje projektnim zahtjevima (MD).
    /// </summary>
    public class ProjektniZahtjevMDController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly ILogger<ProjektniZahtjevMDController> logger;
        private readonly AppSettings appSettings;

        /// <summary>
        /// Konstruktor za ProjektniZahtjevMDController.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Dodatne opcije za straničenje.</param>
        /// <param name="logger">Logger za pračenje i zapisivanje promjena.</param>

        public ProjektniZahtjevMDController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjektniZahtjevMDController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        /// <summary>
        /// Prikaz liste projektnih zahtjeva s opcijama za straničenje i sortiranje.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Prikazuje listu projektnih zahtjeva.</returns>

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.ProjektniZahtjev.AsQueryable();

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


            var prZahtjevi = await query
                         .Select(a => new ProjektniZahtjevViewModel
                         {
                             Id = a.Id,
                             NazivZahtjeva = a.NazivZahtjeva,
                             OpisZahtjeva = a.OpisZahtjeva,
                             PrioritetZahtjevaId = a.PrioritetZahtjeva.Id,
                             PrioritetZahtjevaObj = a.PrioritetZahtjeva,
                             PrioritetZahtjeva = a.PrioritetZahtjeva.NazivPrioritetaZahtjeva,
                             VrstaZahtjevaId = a.VrstaZahtjeva.Id,
                             VrstaZahtjevaObj = a.VrstaZahtjeva,
                             VrstaZahtjeva = a.VrstaZahtjeva.NazivVrsteZahtjeva,
                             ProjektId = a.Projekt.Id,
                             ProjektObj = a.Projekt,
                             Projekt = a.Projekt.NazivProjekt
                         })
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
            .ToListAsync();

            foreach (var zadatak in prZahtjevi)
            {
                var listaNaziva = await ctx.Zadatak
                                      .Where(d => d.ProjektniZahtjevId == zadatak.Id)
                                      .Select(d => d.OpisZadatak)
                                      .ToListAsync();
                string naziv = string.Join(", ", listaNaziva);

                zadatak.NaziviZadataka = naziv;
            }


            var model = new ProjektniZahtjeviViewModel
            {
                ProjektniZahtjevi = prZahtjevi,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        /// <summary>
        /// Prikazuje projektne zahtjeve.
        /// </summary>
        /// <param name="id">ID projektnog zahtjeva.</param>
        /// <param name="filter">Svojstvo po kojem se filtrira.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <param name="viewName">Ime trenutnog pogleda.</param>
        /// <returns></returns>

        public async Task<IActionResult> Show(int id, string filter, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Show))
        {
            var prZahtjev = await ctx.ProjektniZahtjev
                                    .Where(a => a.Id == id)
                                    .Select(a => new ProjektniZahtjevViewModel
                                    {
                                        Id = a.Id,
                                        NazivZahtjeva = a.NazivZahtjeva,
                                        OpisZahtjeva = a.OpisZahtjeva,
                                        PrioritetZahtjevaId = a.PrioritetZahtjeva.Id,
                                        PrioritetZahtjevaObj = a.PrioritetZahtjeva,
                                        PrioritetZahtjeva = a.PrioritetZahtjeva.NazivPrioritetaZahtjeva,
                                        VrstaZahtjevaId = a.VrstaZahtjeva.Id,
                                        VrstaZahtjevaObj = a.VrstaZahtjeva,
                                        VrstaZahtjeva = a.VrstaZahtjeva.NazivVrsteZahtjeva,
                                        ProjektId = a.Projekt.Id,
                                        ProjektObj = a.Projekt,
                                        Projekt = a.Projekt.NazivProjekt
                                    })
                                    .FirstOrDefaultAsync();
            if (prZahtjev == null)
            {
                return NotFound($"Projektni zahtjev s id {id} ne postoji");
            }
            else
            {

                var stavke = await ctx.Zadatak
                                      .Where(d => d.Id == prZahtjev.Id)
                                      .OrderBy(d => d.Id)
                                      .Select(d => new ZadatakViewModel
                                      {
                                          Id = d.Id,
                                          PlanPocetak = d.PlanPocetak,
                                          PlanKraj = d.PlanKraj,
                                          StvarniPocetak = d.StvarniPocetak,
                                          StvarniKraj = d.StvarniKraj,
                                          OpisZadatak = d.OpisZadatak,
                                          PrioritetZadatka = d.PrioritetZadatka.NazivPrioriteta,
                                          StatusZadatka = d.StatusZadatka.Status,
                                          ProjektniZahtjev = d.ProjektniZahtjev.NazivZahtjeva,
                                          Suradnik = d.Suradnik.Ime
                                      })
                                      .ToListAsync();
                prZahtjev.Zadatci = stavke;

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Filter = filter;

                return View(viewName, prZahtjev);
            }
        }


        /// <summary>
        /// Stvaranje drop down listi za odabir određenih atributa.
        /// </summary>
        /// <returns>Drop down liste za odabir određenih atributa.</returns>

        private async Task PrepareDropDownList()
        {
            var prioriteti = await ctx.PrioritetZahtjeva.OrderBy(a => a.NazivPrioritetaZahtjeva)
                .Select(a => new { a.NazivPrioritetaZahtjeva, a.Id })
                .ToListAsync();
            ViewBag.ProjektniZahtjevi = new SelectList(prioriteti,
            nameof(PrioritetZahtjeva.Id), nameof(PrioritetZahtjeva.NazivPrioritetaZahtjeva));

            var vrstaZahtjeva = await ctx.VrstaZahtjeva.OrderBy(a => a.NazivVrsteZahtjeva)
                .Select(a => new { a.NazivVrsteZahtjeva, a.Id })
                .ToListAsync();
            ViewBag.ProjektniZahtjevi = new SelectList(vrstaZahtjeva,
            nameof(VrstaZahtjeva.Id), nameof(VrstaZahtjeva.NazivVrsteZahtjeva));

            var zadatci = await ctx.Zadatak.OrderBy(a => a.Id)
                .Select(a => new { a.OpisZadatak, a.Id })
                .ToListAsync();
            ViewBag.ProjektniZahtjevi = new SelectList(zadatci,
            nameof(Zadatak.Id), nameof(Zadatak.OpisZadatak));
        }

        /// <summary>
        /// Prikaz forme za dodavanje novog projektnog zahtjeva.
        /// </summary>
        /// <returns>Prikaz forme za unos novog projektnog zahtjeva.</returns>

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownList();
            return View();
        }

        /// <summary>
        /// Dodaje novi projektni zahtjev u bazu podataka.
        /// </summary>
        /// <param name="model">Dohvačanje podataka o projektnom zahtjevu.</param>
        /// <returns>Preusmjerava na akciju Edit na kon dodavanja projektnog zahtjeva.</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjektniZahtjevViewModel model)
        {
            if (ModelState.IsValid)
            {
                ProjektniZahtjev a = new ProjektniZahtjev();
                //a.Id = model.Id;
                a.NazivZahtjeva = model.NazivZahtjeva;
                a.OpisZahtjeva = model.OpisZahtjeva;
                a.PrioritetZahtjevaId = model.PrioritetZahtjevaId;
                a.VrstaZahtjevaId = model.VrstaZahtjevaId;
                a.ProjektId = model.ProjektId;


                foreach (var zadatak in model.Zadatci)
                {
                    Zadatak noviZadatak = new Zadatak();
                    noviZadatak.PlanPocetak = zadatak.PlanPocetak;
                    noviZadatak.PlanKraj = zadatak.PlanKraj;
                    noviZadatak.StvarniPocetak = zadatak.StvarniPocetak;
                    noviZadatak.StvarniKraj = zadatak.StvarniKraj;
                    noviZadatak.OpisZadatak = zadatak.OpisZadatak;
                    noviZadatak.PrioritetZadatka = zadatak.PrioritetZadatkaObj;
                    noviZadatak.ProjektniZahtjev = zadatak.ProjektniZahtjevObj;
                    noviZadatak.StatusZadatka = zadatak.StatusZadatkaObj;
                    noviZadatak.Suradnik = zadatak.SuradnikObj;

                    a.Zadatak.Add(noviZadatak);
                }

                try
                {
                    ctx.Add(a);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation(new EventId(1000), $"MD projektni zahtjev  {a.NazivZahtjeva} dodan.");
                    TempData[Constants.Message] = $"Dokument uspješno dodan. Id={a.Id}";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Edit), new { id = a.Id });

                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja novog projektnog zahtjeva: {0}", exc.CompleteExceptionMessage());
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
        /// Briše projektni zahtjev s određenim ID-om iz baze podataka. 
        /// </summary>
        /// <param name="Id">ID projektnog zahtjeva.</param>
        /// <param name="filter">Svojstvo po kojem se filtrira.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Rezultat brisanja projektnog zahtjeva.</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int Id, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            var prZahtjev = await ctx.ProjektniZahtjev
                                    .Where(a => a.Id == Id)
                                    .SingleOrDefaultAsync();
            if (prZahtjev != null)
            {
                try
                {
                    string naziv = prZahtjev.NazivZahtjeva;
                    ctx.Remove(prZahtjev);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Projektni zahtjev: {naziv} uspješno obrisan");
                    TempData[Constants.Message] = $"Projektni zahtjev {prZahtjev.Id} uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom brisanja zahtjeva: " + exc.CompleteExceptionMessage());
                    TempData[Constants.Message] = "Pogreška prilikom brisanja zahtjeva: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
            }
            else
            {
                TempData[Constants.Message] = "Ne postoji zahtjev s id-om: " + Id;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { filter, page, sort, ascending });
        }

        /// <summary>
        /// Pomoćna funkcija za uređivanje postojećeg projektnog zahtjeva u bazi podataka.
        /// </summary>
        /// <param name="id">ID projektnog zahtjeva.</param>
        /// <param name="filter">Svojstvo po kojem se filtrira.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Preusmjerava na akciju Show nakon uređivanja projektnog zahtjeva.</returns>


        [HttpGet]
        public Task<IActionResult> Edit(int id, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            return Show(id, filter, page, sort, ascending, viewName: nameof(Edit));
        }

        /// <summary>
        /// Uređuje postojeći projektni zahtjev u bazi podataka.
        /// </summary>
        /// <param name="model">Dohvačanje podataka o projektnom zahtjevu.</param>
        /// <param name="filter">Svojstvo po kojem se filtrira.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Preusmjerava na akciju Edit nakon uređivanja projektnog zahtjeva.</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjektniZahtjevViewModel model, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            ViewBag.Filter = filter;
            if (ModelState.IsValid)
            {
                var prZahtjev = await ctx.ProjektniZahtjev
                                         .Include(a => a.Zadatak)
                                         .Where(a => a.Id == model.Id)
                                         .FirstOrDefaultAsync();
                if (prZahtjev == null)
                {
                    logger.LogWarning("Ne postoji projektni zahtjev s oznakom: {0} ", model.Id);
                    return NotFound("Ne postoji projektni zahtjev s id-om: " + model.Id);
                }


                prZahtjev.NazivZahtjeva = model.NazivZahtjeva;
                prZahtjev.OpisZahtjeva = model.OpisZahtjeva;
                prZahtjev.PrioritetZahtjeva = model.PrioritetZahtjevaObj;
                prZahtjev.VrstaZahtjeva = model.VrstaZahtjevaObj;
                prZahtjev.Projekt = model.ProjektObj;

                List<int> idZadataka = model.Zadatci
                                          .Where(a => a.Id > 0)
                                          .Select(a => a.Id)
                                          .ToList();
                ctx.RemoveRange(prZahtjev.Zadatak.Where(d => !idZadataka.Contains(d.Id)));

                foreach (var zadatak in model.Zadatci)
                {
                    Zadatak noviZadatak; 
                    if (zadatak.Id > 0)
                    {
                        noviZadatak = prZahtjev.Zadatak.First(d => d.Id == zadatak.Id);
                    }
                    else
                    {
                        noviZadatak = new Zadatak();
                        prZahtjev.Zadatak.Add(noviZadatak);
                    }
                    noviZadatak.PlanPocetak = zadatak.PlanPocetak;
                    noviZadatak.PlanKraj = zadatak.PlanKraj;
                    noviZadatak.StvarniPocetak = zadatak.StvarniPocetak;
                    noviZadatak.StvarniKraj = zadatak.StvarniKraj;
                    noviZadatak.OpisZadatak = zadatak.OpisZadatak;
                    noviZadatak.PrioritetZadatka = zadatak.PrioritetZadatkaObj;
                    noviZadatak.ProjektniZahtjev = zadatak.ProjektniZahtjevObj;
                    noviZadatak.StatusZadatka = zadatak.StatusZadatkaObj;
                    noviZadatak.Suradnik = zadatak.SuradnikObj;
                    ctx.Update(noviZadatak);
                }


                try
                {
                    ctx.Update(prZahtjev);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"(MD) projektni zahtjev: {prZahtjev.NazivZahtjeva} uspješno ažuriran");
                    TempData[Constants.Message] = $"Dokument {prZahtjev.Id} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Edit), new
                    {
                        id = prZahtjev.Id,
                        filter,
                        page,
                        sort,
                        ascending
                    });

                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom ažuriranja projektnog zahtjeva: " + exc.CompleteExceptionMessage());
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
        /// Prikazuje detalje o projektnog zahtjevu.
        /// </summary>
        /// <param name="id">ID projektnog zahtjeva.</param>
        /// <returns>Prikazuje detalje o projektnom zahtjevu.</returns>


        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var prZahtjev = await ctx.ProjektniZahtjev
                                  .Where(a => a.Id == id)
                                  .Select(a => new ProjektniZahtjevViewModel
                                  {
                                      Id = a.Id,
                                      NazivZahtjeva = a.NazivZahtjeva,
                                      OpisZahtjeva = a.OpisZahtjeva,
                                      PrioritetZahtjeva = a.PrioritetZahtjeva.NazivPrioritetaZahtjeva,
                                      VrstaZahtjeva = a.VrstaZahtjeva.NazivVrsteZahtjeva,
                                      Projekt = a.Projekt.NazivProjekt
                                  })
                                  .SingleOrDefaultAsync();
            if (prZahtjev != null)
            {
                return PartialView(prZahtjev);
            }
            else
            {
                return NotFound($"Neispravan id projektnog zahtjeva: {id}");
            }
        }
    }
}
