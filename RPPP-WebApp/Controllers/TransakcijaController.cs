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
    public class TransakcijaController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly AppSettings appData;
        private readonly ILogger<TransakcijaController> logger;
        public TransakcijaController(Rppp04Context ctx,
        IOptionsSnapshot<AppSettings> options, ILogger<TransakcijaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;

        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;
            var query = ctx.Transakcija
                .AsNoTracking();
            int count = query.Count();
            if(count == 0)
            {
                logger.LogInformation("Ne postoji niti jedna transakcija");
                TempData[Constants.Message] = "Ne postoji niti jedna transakcija.";
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
            var transakcije =  query
                  .Select(m => new TransakcijaViewModel
                  {
                      Id = m.Id,
                      Iznos = m.Iznos ?? 0,
                      Iban = m.Iban,
                      DatumVrijeme = m.DatumVrijeme,
                      KarticaFrom = m.KarticaProjekta != null ? m.KarticaProjekta.Id : 0,
                      KarticaTo = m.KarticaProjekta != null ? m.KarticaProjekta.Id : 0,
                      Vrsta = m.VrstaTransakcije
                  }
                   )
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToList();
            var model = new TransakcijeViewModel
            {
                Transakcije = transakcije,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int page = 1, int sort = 1, bool ascending = true)
        {
            TempData["page"] = page;
            TempData["sort"]= sort;
            TempData["ascending"] = ascending;
            await PrepareDropDownLists();
            return View();
        }


        [HttpPost]
        public IActionResult Create(Transakcija Transakcija, int page, int sort, bool ascending)
        {
            logger.LogTrace(JsonSerializer.Serialize(Transakcija));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(Transakcija);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Nova transakcija dodana.");

                    TempData[Constants.Message] = "Nova transakcija dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove transakcije: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(Transakcija);
                }
            }
            else
            {
                return View(Transakcija);
            }
        }


        [HttpPost]
        public IActionResult Delete(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var Transakcija = ctx.Transakcija.Find(Id);
            if (Transakcija != null)
            {
                try
                {
                    var id = Transakcija.Id;
                    ctx.Remove(Transakcija);
                    ctx.SaveChanges();
                    logger.LogInformation($"Transakcija {id} uspješno obrisana.");
                    TempData[Constants.Message] = $"Transakcija {id} uspješno obrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja transakcije: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja transakcije: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji transakcija s oznakom: {0} ", Id);
                TempData[Constants.Message] = "Ne postoji transakcija s oznakom: " + Id;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });

        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id, bool? ignoreThisParameter, int page = 1, int sort = 1, bool ascending = true)
        {
            var Transakcija = ctx.Transakcija.AsNoTracking().Where(d => d.Id == id).SingleOrDefault();
            if (Transakcija == null)
            {
                logger.LogWarning("Ne postoji transakcija s oznakom: {0} ", id);
                return NotFound("Ne postoji transakcija s oznakom: " + id);
            }
            else
            {
                await PrepareDropDownLists();
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(Transakcija);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {

            try
            {
                Transakcija Transakcija = await ctx.Transakcija
                                  .Where(d => d.Id == id)
                                  .FirstOrDefaultAsync();
                if (Transakcija == null)
                {
                    return NotFound("Neispravan id transakcije: " + id);
                }

                if(await TryUpdateModelAsync<Transakcija>(Transakcija, "", d => d.Iznos,
                    d => d.Iban, d => d.DatumVrijeme, d => d.KarticaProjektaId, d => d.KarticaProjektaId1))
                {

                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        ctx.SaveChanges();
                        TempData[Constants.Message] = "Transakcija ažurirana.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View("Edit", Transakcija);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o transakciji nije moguće povezati s forme");
                    return View("Edit", Transakcija);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), id);
            }
        }
        private async Task PrepareDropDownLists()
        {
            ViewBag.KarticaProjektaId = new SelectList(await ctx.KarticaProjekta.ToListAsync(), "Id", "Id");

            ViewBag.VrstaTransakcijeId = new SelectList(await ctx.VrstaTransakcije.ToListAsync(), "Id", "Vrsta");
        }
    }
}
