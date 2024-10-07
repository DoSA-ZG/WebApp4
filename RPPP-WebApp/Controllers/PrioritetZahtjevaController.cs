using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Extensions;
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
    /// Kontroler za upravljanje prioritetima zahtjeva.
    /// </summary>
    public class PrioritetZahtjevaController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly AppSettings appData;
        private readonly ILogger<PrioritetZahtjevaController> logger;

        /// <summary>
        /// Konstruktor za PrioritetZahtjevaController.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Dodatne opcije za straničenje.</param>
        /// <param name="logger">Logger za pračenje i zapisivanje promjena.</param>

        public PrioritetZahtjevaController(Rppp04Context ctx,
        IOptionsSnapshot<AppSettings> options, ILogger<PrioritetZahtjevaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        /// <summary>
        /// Prikaz liste prioriteta zahtjeva s opcijama za straničenje i sortiranje.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Prikazuje listu prioriteta zahtjeva.</returns>

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.PrioritetZahtjeva.AsNoTracking();

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

            var prioriteti = query.Skip((page - 1) * pagesize)
                                  .Take(pagesize)
                                  .ToList();

            var model = new PrioritetZahtjevaViewModel
            {
                PrioritetiZahtjeva = prioriteti,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        /// <summary>
        /// Pomoćna funkcija za stvaranje novog prioriteta zahtjeva.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Vraća view nakon stvaranja novog prioriteta zahtjeva.</returns>

        [HttpGet]
        public async Task<IActionResult> Create(int page = 1, int sort = 1, bool ascending = true)
        {
            //await PrepareDropDownLists();
            return View();
        }

        /// <summary>
        /// Stvara novi prioritet zahtjeva.
        /// </summary>
        /// <param name="prioritet">Podatci o prioritetu zahtjeva.</param>
        /// <returns>Preusmjerava na akciju Index nakon stvaranja novog prioriteta zahtjeva.</returns>

        [HttpPost]
        public IActionResult Create(PrioritetZahtjeva prioritet)
        {

            logger.LogTrace(JsonSerializer.Serialize(prioritet));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(prioritet);
                    ctx.SaveChanges();

                    logger.LogInformation(new EventId(1000), $"Prioritet zahtjeva {prioritet.NazivPrioritetaZahtjeva} dodan.");

                    TempData[Constants.Message] = $"Prioritet zahtjeva {prioritet.NazivPrioritetaZahtjeva} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog prioriteta zahtjeva: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    //await PrepareDropDownList();
                    return View(prioritet);
                }
            }
            else
            {
                //await PrepareDropDownList();
                return View(prioritet);
            }

        }

        /// <summary>
        /// Briše određeni prioritet zahtjeva.
        /// </summary>
        /// <param name="Id">ID prioriteta zahtjeva.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Rezultat brisanja prioriteta zahtjeva.</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var prioritet = ctx.PrioritetZahtjeva.Find(Id);
            if (prioritet != null)
            {
                try
                {
                    var id = prioritet.Id;
                    ctx.Remove(prioritet);
                    ctx.SaveChanges();
                    logger.LogInformation($"Prioritet zahtjeva {prioritet.NazivPrioritetaZahtjeva} uspješno obrisan.");
                    TempData[Constants.Message] = $"Prioritet zahtjeva {id} uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja prioriteta zahtjeva: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja prioriteta zahtjeva: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning($"Ne postoji prioritet zahtjeva s oznakom: {Id}.");
                TempData[Constants.Message] = $"Ne postoji prioritet zahtjeva s oznakom: {Id}.";
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }

        /// <summary>
        /// Uređuje određeni prioritet zahtjeva.
        /// </summary>
        /// <param name="Id">ID prioriteta zahtjeva.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Parcijalni pogled nakon uređivanja prioriteta zahtjeva.</returns>

        [HttpGet]
        public async Task<IActionResult> Edit(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var prioritet = ctx.PrioritetZahtjeva.AsNoTracking().Where(a => a.Id == Id).SingleOrDefault();
            if (prioritet == null)
            {
                logger.LogWarning($"Ne postoji prioritet zahtjeva s oznakom: {Id}.");
                return NotFound($"Ne postoji prioritet zahtjeva s oznakom: {Id}.");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return PartialView(prioritet);
            }
        }

        /// <summary>
        /// Nadopunjava određeni prioritet zahtjeva.
        /// </summary>
        /// <param name="id">ID prioriteta zahtjeva.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Preusmjerava na akciju Index nakon nadopunjavanja prioriteta zahtjeva.</returns>

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                PrioritetZahtjeva prioritet = await ctx.PrioritetZahtjeva
                                 .Where(a => a.Id == id)
                                 .FirstOrDefaultAsync();
                if (prioritet == null)
                {
                    return NotFound("Neispravan Id prioriteta zahtjeva: " + id);
                }

                if (await TryUpdateModelAsync<PrioritetZahtjeva>(prioritet, "", a => a.NazivPrioritetaZahtjeva,
                    a => a.StupanjPrioriteta))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        ctx.SaveChanges();
                        logger.LogInformation($"Prioritet zahtjeva: {prioritet.NazivPrioritetaZahtjeva} uspješno ažuriran");
                        TempData[Constants.Message] = $"Prioritet zahtjeva {prioritet.NazivPrioritetaZahtjeva} ažuriran.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        logger.LogError($"Pogreška prilikom ažuriranja prioriteta zahtjeva: {exc.CompleteExceptionMessage()}");
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(prioritet);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o prioritetu zahtjeva nije moguće povezati s formom");
                    return View(prioritet);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Update), id);
            }
        }
    }
}
