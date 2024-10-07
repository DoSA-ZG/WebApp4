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
    /// Kontroler za upravljanje vrstama zahtjeva.
    /// </summary>
    public class VrstaZahtjevaController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly AppSettings appData;
        private readonly ILogger<VrstaZahtjevaController> logger;

        /// <summary>
        /// Kontroller za VrstaZahtjevaController.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Dodatne opcije za straničenje.</param>
        /// <param name="logger">Logger za pračenje i zapisivanje promjena.</param>
        public VrstaZahtjevaController(Rppp04Context ctx,
        IOptionsSnapshot<AppSettings> options, ILogger<VrstaZahtjevaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        /// <summary>
        /// Prikaz liste vrsta zahtjeva s opcijama za straničenje i sortiranje.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Prikazuje listu vrsta zahtjeva.</returns>


        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.VrstaZahtjeva.AsNoTracking();

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

            var vrsteZahtjeva = query.Skip((page - 1) * pagesize)
                                  .Take(pagesize)
                                  .ToList();

            var model = new VrstaZahtjevaViewModel
            {
                VrsteZahtjeva = vrsteZahtjeva,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        /// <summary>
        /// Pomoćna funkcija za stvaranje nove vrste zahtjeva.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Vraća view nakon stvaranja nove vrste zahtjeva.</returns>

        [HttpGet]
        public async Task<IActionResult> Create(int page = 1, int sort = 1, bool ascending = true)
        {
            return View();
        }

        /// <summary>
        /// Stvara novu vrstu zahtjeva.
        /// </summary>
        /// <param name="vrZahtjeva">Podatci o vrsti zahtjeva.</param>
        /// <returns>Preusmjerava na akciju Index nakon stvaranja nove vrste zahtjeva.</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(VrstaZahtjeva vrZahtjeva)
        {

            logger.LogTrace(JsonSerializer.Serialize(vrZahtjeva));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(vrZahtjeva);
                    ctx.SaveChanges();

                    logger.LogInformation(new EventId(1000), $"Vrsta zahtjeva {vrZahtjeva.NazivVrsteZahtjeva} dodana.");

                    TempData[Constants.Message] = $"Vrsta zahtjeva {vrZahtjeva.NazivVrsteZahtjeva} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje nove vrste zahtjeva: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrZahtjeva);
                }
            }
            else
            {
                return View(vrZahtjeva);
            }

        }

        /// <summary>
        /// Briše određenu vrstu zahtjeva.
        /// </summary>
        /// <param name="Id">ID vrste zahtjeva.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Rezultat brisanja vrste zahtjeva.</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var vrZahtjeva = ctx.VrstaZahtjeva.Find(Id);
            if (vrZahtjeva != null)
            {
                try
                {
                    var id = vrZahtjeva.Id;
                    ctx.Remove(vrZahtjeva);
                    ctx.SaveChanges();
                    logger.LogInformation($"Vrsta zahtjeva {vrZahtjeva.NazivVrsteZahtjeva} uspješno obrisana.");
                    TempData[Constants.Message] = $"Vrsta zahtjeva {id} uspješno obrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja vrste zahtjeva: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja vrste zahtjeva: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning($"Ne postoji vrsta zahtjeva s oznakom: {Id}.");
                TempData[Constants.Message] = $"Ne postoji vrsta zahtjeva s oznakom: {Id}.";
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }

        /// <summary>
        /// Uređuje određenu vrstu zahtjeva.
        /// </summary>
        /// <param name="Id">ID prioriteta zahtjeva.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Parcijalni pogled nakon uređivanja vrste zahtjeva.</returns>

        [HttpGet]
        public async Task<IActionResult> Edit(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var vrstaZahtjeva = ctx.VrstaZahtjeva.AsNoTracking().Where(a => a.Id == Id).SingleOrDefault();
            if (vrstaZahtjeva == null)
            {
                logger.LogWarning($"Ne postoji vrsta zahtjeva s oznakom: {Id}.");
                return NotFound($"Ne postoji vrsta zahtjeva s oznakom: {Id}.");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return PartialView(vrstaZahtjeva);
            }
        }

        /// <summary>
        /// Nadopunjava određenu vrstu zahtjeva.
        /// </summary>
        /// <param name="id">ID prioriteta zahtjeva.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Preusmjerava na akciju Index nakon nadopunjavanja vrste zahtjeva.</returns>

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                VrstaZahtjeva vrZahtjeva = await ctx.VrstaZahtjeva
                                 .Where(a => a.Id == id)
                                 .FirstOrDefaultAsync();
                if (vrZahtjeva == null)
                {
                    return NotFound("Neispravan Id vrste zahtjeva: " + id);
                }

                if (await TryUpdateModelAsync<VrstaZahtjeva>(vrZahtjeva, "", a => a.NazivVrsteZahtjeva))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        ctx.SaveChanges();
                        logger.LogInformation($"Vrsta zahtjeva: {vrZahtjeva.NazivVrsteZahtjeva} uspješno ažurirana");
                        TempData[Constants.Message] = $"Vrsta zahtjeva {vrZahtjeva.NazivVrsteZahtjeva} ažurirana.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        logger.LogError($"Pogreška prilikom ažuriranja vrste zahtjeva: {exc.CompleteExceptionMessage()}");
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(vrZahtjeva);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o vrsti zahtjeva nije moguće povezati s formom");
                    return View(vrZahtjeva);
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
