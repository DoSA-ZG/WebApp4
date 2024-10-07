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
    /// Kontroler za prioritet zadatka.
    /// </summary>
    public class PrioritetZadatkaController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly AppSettings appData;
        private readonly ILogger<PrioritetZadatkaController> logger;

        /// <summary>
        /// Konstruktor za PrioritetZadatkaController
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Dodatne opcije za straničenje.</param>
        /// <param name="logger">Logger za pračenje i zapisivanje promjena.</param>
        public PrioritetZadatkaController(Rppp04Context ctx,
        IOptionsSnapshot<AppSettings> options, ILogger<PrioritetZadatkaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        /// <summary>
        /// Prikaz liste prioriteta zadataka s opcijama za straničenje i sortiranje.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Prikazuje listu prioriteta zadataka.</returns>

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.PrioritetZadatka.AsNoTracking();

            int count = query.Count();

            if (count == 0)
            {
                logger.LogInformation("Ne postoji niti jedan projektni zahtjev.");
                TempData[Constants.Message] = "Ne postoji niti jedan projektni zahtjev.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Create));
            }

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

            var model = new PrioritetiZadatakaViewModel
            {
                PrioritetiZadataka = prioriteti,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        /// <summary>
        /// Pomoćna funkcija za stvaranje novog prioriteta zadatka.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Vraća view nakon stvaranja novog prioriteta zadatka.</returns>

        [HttpGet]
        public async Task<IActionResult> Create(int page = 1, int sort = 1, bool ascending = true)
        {
            //await PrepareDropDownLists();
            return View();
        }

        [HttpPost]
        public IActionResult Create(PrioritetZadatka prioritet)
        {

            logger.LogTrace(JsonSerializer.Serialize(prioritet));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(prioritet);
                    ctx.SaveChanges();

                    logger.LogInformation(new EventId(1000), $"Prioritet zadatka {prioritet.NazivPrioriteta} dodan.");

                    TempData[Constants.Message] = $"Prioritet zadatka {prioritet.NazivPrioriteta} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog prioriteta zadatka: {0}", exc.CompleteExceptionMessage());
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
        /// Briše određeni prioritet zadatka.
        /// </summary>
        /// <param name="Id">ID prioriteta zadatka.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Rezultat brisanja prioriteta zadatka.</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var prioritet = ctx.PrioritetZadatka.Find(Id);
            if (prioritet != null)
            {
                try
                {
                    var id = prioritet.Id;
                    ctx.Remove(prioritet);
                    ctx.SaveChanges();
                    logger.LogInformation($"Prioritet zadatka {prioritet.NazivPrioriteta} uspješno obrisan.");
                    TempData[Constants.Message] = $"Prioritet zadatka {id} uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja prioriteta zadatka: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja prioriteta zadatka: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning($"Ne postoji prioritet zadatka s oznakom: {Id}.");
                TempData[Constants.Message] = $"Ne postoji prioritet zadatak s oznakom: {Id}.";
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }

        /// <summary>
        /// Uređuje određen prioritet zadatka.
        /// </summary>
        /// <param name="Id">ID prioriteta zadatka.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Parcijalni pogled nakon uređivanja prioriteta zadatka.</returns>

        [HttpGet]
        public async Task<IActionResult> Edit(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var prioritet = ctx.PrioritetZadatka.AsNoTracking().Where(a => a.Id == Id).SingleOrDefault();
            if (prioritet == null)
            {
                logger.LogWarning($"Ne postoji prioritet zadatka s oznakom: {Id}.");
                return NotFound($"Ne postoji prioritet zadatka s oznakom: {Id}.");
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
        /// Nadopunjava određen prioritet zadatka.
        /// </summary>
        /// <param name="id">ID prioriteta zadatka.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Smjer sortiranja.</param>
        /// <returns>Preusmjerava na akciju Index nakon nadopunjavanja prioriteta zadatka.</returns>

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                PrioritetZadatka prioritet = await ctx.PrioritetZadatka
                                 .Where(a => a.Id == id)
                                 .FirstOrDefaultAsync();
                if (prioritet == null)
                {
                    return NotFound("Neispravan Id prioriteta zadatka: " + id);
                }

                if (await TryUpdateModelAsync<PrioritetZadatka>(prioritet, "", a => a.NazivPrioriteta,
                    a => a.StupanjPrioriteta))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        ctx.SaveChanges();
                        logger.LogInformation($"Prioritet zadatka: {prioritet.NazivPrioriteta} uspješno ažuriran");
                        TempData[Constants.Message] = $"Prioritet zadatka {prioritet.NazivPrioriteta} ažuriran.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        logger.LogError($"Pogreška prilikom ažuriranja prioriteta zadatka: {exc.CompleteExceptionMessage()}");
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(prioritet);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o prioritetu zadatka nije moguće povezati s formom");
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
