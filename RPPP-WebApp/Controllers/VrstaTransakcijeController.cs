using Microsoft.AspNetCore.Mvc;
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
    public class VrstaTransakcijeController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly AppSettings appData;
        private readonly ILogger<VrstaTransakcijeController> logger;

        public VrstaTransakcijeController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<VrstaTransakcijeController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;
            var query = ctx.VrstaTransakcije
                .AsNoTracking();
            int count = query.Count();

            if (count == 0)
            {
                logger.LogInformation("Ne postoji niti jedna vrsta transakcije");
                TempData[Constants.Message] = "Ne postoji niti jedna vrsta transakcije.";
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

            var vrsteTransakcije = query
                .Select(m => new VrstaTransakcijeViewModel
                {
                    Id = m.Id,
                    Vrsta = m.Vrsta,
                    
                })
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToList();

            var model = new VrsteTransakcijeViewModel
            {
                VrsteTransakcije = vrsteTransakcije,
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
        public IActionResult Create(VrstaTransakcije vrstaTransakcije, int page, int sort, bool ascending)
        {
            logger.LogTrace(JsonSerializer.Serialize(vrstaTransakcije));

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(vrstaTransakcije);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Nova vrsta transakcije dodana.");

                    TempData[Constants.Message] = "Nova vrsta transakcije dodana.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove vrste transakcije: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    return View(vrstaTransakcije);
                }
            }
            else
            {
                return View(vrstaTransakcije);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var vrstaTransakcije = await ctx.VrstaTransakcije.AsNoTracking().Where(d => d.Id == id).SingleOrDefaultAsync();

            if (vrstaTransakcije == null)
            {
                logger.LogWarning("Ne postoji vrsta transakcije s oznakom: {0} ", id);
                return NotFound("Ne postoji vrsta transakcije s oznakom: " + id);
            }
            else
            {
                //await PrepareDropDownLists();
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(vrstaTransakcije);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                var vrstaTransakcije = await ctx.VrstaTransakcije
                    .Where(d => d.Id == id)
                    .FirstOrDefaultAsync();

                if (vrstaTransakcije == null)
                {
                    return NotFound("Neispravan id vrste transakcije: " + id);
                }

                if (await TryUpdateModelAsync<VrstaTransakcije>(vrstaTransakcije, "", d => d.Vrsta))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;

                    try
                    {
                        ctx.SaveChanges();
                        TempData[Constants.Message] = "Vrsta transakcije ažurirana.";
                        TempData[Constants.ErrorOccurred] = false;

                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View("Edit", vrstaTransakcije);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o vrsti transakcije nije moguće povezati s forme");
                    return View("Edit", vrstaTransakcije);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), id);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var vrstaTransakcije = ctx.VrstaTransakcije.Find(id);

            if (vrstaTransakcije != null)
            {
                try
                {
                    var vrstaId = vrstaTransakcije.Id;
                    ctx.Remove(vrstaTransakcije);
                    ctx.SaveChanges();
                    logger.LogInformation($"Vrsta transakcije {vrstaId} uspješno obrisana.");

                    TempData[Constants.Message] = $"Vrsta transakcije {vrstaId} uspješno obrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja vrste transakcije: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja vrste transakcije: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji vrsta transakcije s oznakom: {0} ", id);
                TempData[Constants.Message] = "Ne postoji vrsta transakcije s oznakom: " + id;
                TempData[Constants.ErrorOccurred] = true;
            }

            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }
        /*
        private async Task PrepareDropDownLists()
        {
            // Možda mi zatreba nešto ovog tipa
            // ViewBag.SomeDropDownList = new SelectList(await ctx.SomeEntities.ToListAsync(), "Id", "Name");
        }
        */
    }
}