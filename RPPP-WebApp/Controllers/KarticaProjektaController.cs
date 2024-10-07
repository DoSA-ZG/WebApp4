using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RPPP_WebApp.Controllers
{
    public class KarticaProjektaController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly AppSettings appData;
        private readonly ILogger<KarticaProjektaController> logger;

        public KarticaProjektaController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<KarticaProjektaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;
            var query = ctx.KarticaProjekta
                .AsNoTracking();
            int count = query.Count();

            if (count == 0)
            {
                logger.LogInformation("Ne postoji niti jedna kartica projekta");
                TempData[Constants.Message] = "Ne postoji niti jedna kartica projekta.";
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

            var karticeProjekta = query
                .Select(m => new KarticaProjektaViewModel
                {
                    Id = m.Id,
                    ProjektId = m.ProjektId,
                })
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToList();

            var model = new KarticeProjektaViewModel
            {
                KarticeProjekta = karticeProjekta,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create(int page = 1, int sort = 1, bool ascending = true)
        {
            TempData["page"] = page;
            TempData["sort"] = sort;
            TempData["ascending"] = ascending;

            return View();
        }

        [HttpPost]
        public IActionResult Create(KarticaProjekta karticaProjekta, int page, int sort, bool ascending)
        {
            logger.LogTrace(JsonSerializer.Serialize(karticaProjekta));

            // Provjeri postoji li kartica s istim projekt id-om
            var postojecaKarticaProjekta = ctx.KarticaProjekta
                .Where(k => k.ProjektId == karticaProjekta.ProjektId)
                .FirstOrDefault();

            if (postojecaKarticaProjekta != null)
            {
                // Postoji kartica projekta sa traženim id-om
                TempData["ErrorMessage"] = "kartica projekta za ovaj projekt već postoji.";
                
                // Vrati na view
                return RedirectToAction(nameof(Create));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(karticaProjekta);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Nova kartica projekta dodana.");

                    TempData[Constants.Message] = "Nova kartica projekta dodana.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove kartice projekta: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    return View(karticaProjekta);
                }
            }
            else
            {
                return View(karticaProjekta);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var karticaProjekta = await ctx.KarticaProjekta.AsNoTracking().Where(d => d.Id == id).SingleOrDefaultAsync();

            if (karticaProjekta == null)
            {
                logger.LogWarning("Ne postoji kartica projekta s oznakom: {0} ", id);
                return NotFound("Ne postoji kartica projekta s oznakom: " + id);
            }
            else
            {
                await PrepareDropDownLists();
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(karticaProjekta);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                var karticaProjekta = await ctx.KarticaProjekta
                    .Where(d => d.Id == id)
                    .FirstOrDefaultAsync();

                if (karticaProjekta == null)
                {
                    return NotFound("Neispravan id kartice projekta: " + id);
                }

                if (await TryUpdateModelAsync<KarticaProjekta>(karticaProjekta, "", d => d.ProjektId))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;

                    try
                    {
                        ctx.SaveChanges();
                        TempData[Constants.Message] = "Kartica projekta ažurirana.";
                        TempData[Constants.ErrorOccurred] = false;

                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View("Edit", karticaProjekta);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o kartici projekta nije moguće povezati s forme");
                    return View("Edit", karticaProjekta);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Update), id);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var karticaProjekta = ctx.KarticaProjekta.Find(id);

            if (karticaProjekta != null)
            {
                try
                {
                    var karticaId = karticaProjekta.Id;
                    ctx.Remove(karticaProjekta);
                    ctx.SaveChanges();
                    logger.LogInformation($"Kartica projekta {karticaId} uspješno obrisana.");

                    TempData[Constants.Message] = $"Kartica projekta {karticaId} uspješno obrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja kartice projekta: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja kartice projekta: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji kartica projekta s oznakom: {0} ", id);
                TempData[Constants.Message] = "Ne postoji kartica projekta s oznakom: " + id;
                TempData[Constants.ErrorOccurred] = true;
            }

            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }
        
        private async Task PrepareDropDownLists()
        {
            ViewBag.Projekti = new SelectList(await ctx.Projekt.ToListAsync(), "Id", "Naziv");
        }
        
    }
}
