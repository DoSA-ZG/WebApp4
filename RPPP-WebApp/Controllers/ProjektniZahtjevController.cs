using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System;
using System.Linq;
using System.Security.Policy;
using System.Text.Json;
using System.Threading.Tasks;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje projektnim zahtjevima.
    /// </summary>
    public class ProjektniZahtjevController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly AppSettings appData;
        private readonly ILogger<ProjektniZahtjevController> logger;

        /// <summary>
        /// Konstruktor za ProjektniZahtjevController.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Dodatne opcije za straničenje.</param>
        /// <param name="logger">Logger za pračenje i zapisivanje promjena.</param>
        public ProjektniZahtjevController(Rppp04Context ctx,
        IOptionsSnapshot<AppSettings> options, ILogger<ProjektniZahtjevController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        /// <summary>
        /// Prikaz liste projektnih zahtjeva s opcijama za straničenje i sortiranje.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Prikazuje listu projektnih zahtjeva.</returns>

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.ProjektniZahtjev.AsNoTracking();

            int count = query.Count();

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
            var zahtjevi = query.Select(a => new ProjektniZahtjevViewModel
            {
                Id = a.Id,
                NazivZahtjeva = a.NazivZahtjeva,
                OpisZahtjeva = a.OpisZahtjeva,
                PrioritetZahtjeva = a.PrioritetZahtjeva.NazivPrioritetaZahtjeva,
                VrstaZahtjeva = a.VrstaZahtjeva.NazivVrsteZahtjeva,


            })
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .ToList();
            var model = new ProjektniZahtjeviViewModel
            {
                ProjektniZahtjevi = zahtjevi,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        /// <summary>
        /// Pomoćna funkcija za dodavanje novog projektnog zahtjeva u bazu podataka.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Preusmjerava na pregled nakon dodavanja novog projektnog zahtjeva.</returns>

        [HttpGet]
        public async Task<IActionResult> Create(int page = 1, int sort = 1, bool ascending = true)
        {
            TempData["page"] = page;
            TempData["sort"] = sort;
            TempData["ascending"] = ascending;
            await PrepareDropDownLists();
            return View();
        }

        /// <summary>
        /// Dodaje novi projektni zahtjev u bazu podataka.
        /// </summary>
        /// <param name="zahtjev">Podatci o projektnom zahtjevu.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Preusmjerava na akciju Index nakon dodavanja novog projektnog zahtjeva.</returns>

        [HttpPost]
        public IActionResult Create(ProjektniZahtjev zahtjev, int page, int sort, bool ascending)
        {
            logger.LogTrace(JsonSerializer.Serialize(zahtjev));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(zahtjev);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Novi projektni zahtjev {zahtjev.NazivZahtjeva} dodan.");

                    TempData[Constants.Message] = $"Novi projektni zahtjev {zahtjev.NazivZahtjeva} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodvanja novog projektnog zahtjeva: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(zahtjev);
                }
            }
            else
            {
                return View(zahtjev);
            }
        }

        /// <summary>
        /// Briše projektni zahtjev iz baze podataka.
        /// </summary>
        /// <param name="Id">ID projektnog zahtjeva.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Preusmjerava na akciju index nakon brisanja projektnog zahtjeva.</returns>

        [HttpPost]
        public IActionResult Delete(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var zahtjev = ctx.ProjektniZahtjev.Find(Id);
            if (zahtjev != null)
            {
                try
                {
                    var id = zahtjev.Id;
                    ctx.Remove(zahtjev);
                    ctx.SaveChanges();
                    logger.LogInformation($"Projektni zahtjev {zahtjev.NazivZahtjeva} uspješno obrisan.");
                    TempData[Constants.Message] = $"Projektni zahtjev {id} uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja projektnog zahtjeva: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja projektnog zahtjeva: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji projektni zadatak s oznakom: {0} ", Id);
                TempData[Constants.Message] = "Ne postoji projektni zadatak s oznakom: " + Id;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }

        /// <summary>
        /// Uređuje postojeći projektni zahtjev u bazi podataka.
        /// </summary>
        /// <param name="Id">ID projektnog zahtjeva.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Preusmjerava na parcijalni pogled nakon uređivanja projektnog zahtjeva.</returns>

        [HttpGet]
        public async Task<IActionResult> Edit(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var zahtjev = ctx.ProjektniZahtjev.AsNoTracking().Where(a => a.Id == Id).SingleOrDefault();
            if (zahtjev == null)
            {
                logger.LogWarning("Ne postoji projektni zadatak s oznakom: {0} ", Id);
                return NotFound("Ne postoji projektni zadatak s oznakom: " + Id);
            }
            else
            {
                await PrepareDropDownLists();
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return PartialView(zahtjev);
            }
        }

        /// <summary>
        /// Dohvaća određeni projektni zahtjev.
        /// </summary>
        /// <param name="id">ID projektnog zahtjeva.</param>
        /// <returns>Preusmjerava na parcijalni pogled nakon dohvaćanja projektnog zahtjeva.</returns>

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var zahtjev = await ctx.ProjektniZahtjev
                        .Where(a => a.Id == id)
                        .Select(a => new ProjektniZahtjevViewModel
                        {
                            Id = a.Id,
                            NazivZahtjeva = a.NazivZahtjeva,
                            OpisZahtjeva = a.OpisZahtjeva,
                            PrioritetZahtjeva = a.PrioritetZahtjeva.NazivPrioritetaZahtjeva,
                            VrstaZahtjeva = a.VrstaZahtjeva.NazivVrsteZahtjeva,
                            Projekt = a.Projekt.NazivProjekt
                        }).SingleOrDefaultAsync();
            if(zahtjev != null)
            {
                return PartialView(zahtjev);
            } else {
                return NotFound($"Neispravan id projektnog zahtjeva: {id}");
            }
        }

        /// <summary>
        /// Stvaranje drop down listi za odabir određenih atributa.
        /// </summary>
        /// <returns>Drop down liste za odabir određenih atributa.</returns>

        private async Task PrepareDropDownLists()
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
    }
}