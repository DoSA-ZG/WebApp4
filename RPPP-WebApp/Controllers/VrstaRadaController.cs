using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Klasa koja predstavlja upravljač nad modelom vrste rada
    /// </summary>
    public class VrstaRadaController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly AppSettings appData;
        private readonly ILogger<VrstaRadaController> logger;
        public VrstaRadaController(Rppp04Context ctx,
        IOptionsSnapshot<AppSettings> options, ILogger<VrstaRadaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;

        }
        /// <summary>
        /// Prikazivanje tablice vrsti rada
        /// </summary>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {


            try
            {
                int pagesize = appData.PageSize;
                var query = ctx.VrstaRada
                    .AsNoTracking();
                int count = query.Count();
                if (count == 0)
                {
                    logger.LogInformation("Ne postoji niti jedna vrsta rada");
                    TempData[Constants.Message] = "Ne postoji niti jedna vrsta rada.";
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
                var radovi = query
                      .Select(m => new VrstaRada
                      {
                          Id = m.Id,
                          VrstaRada1 = m.VrstaRada1

                      }
                       )
                      .Skip((page - 1) * pagesize)
                      .Take(pagesize)
                      .ToList();
                var model = new VrsteRadaViewModel
                {
                    radovi = radovi,
                    PagingInfo = pagingInfo
                };
                return View(model);
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom prikaza vrsti radova: {0}", exc.CompleteExceptionMessage());
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                return View();
            }
        }
        /// <summary>
        /// Prikaz forme za stvaranje nove vrste rada
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {

            return View();
        }
        /// <summary>
        /// Dodavanje nove vrste rada 
        /// </summary>
        /// <param name="rad">vrsta rada koja se dodaje</param>
        /// <returns></returns>

        [HttpPost]
        public IActionResult Create(VrstaRada rad)
        {
            logger.LogTrace(JsonSerializer.Serialize(rad));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(rad);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Nova vrsta rada dodana.");

                    TempData[Constants.Message] = $"Nova vrsta rada dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove vrte rada: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(rad);
                }
            }
            else
            {
                return View(rad);
            }
        }
        /// <summary>
        /// Prikaz forme za ažuriranje vrste rada
        /// </summary
        /// <param name="id">id vrste rada koji se ažurira</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var rad = await ctx.VrstaRada.AsNoTracking().Where(d => d.Id == id).SingleOrDefaultAsync();
            if (rad == null)
            {
                logger.LogWarning("Ne postoji rad s oznakom: {0} ", id);
                return NotFound("Ne postoji rad s oznakom: " + id);
            }
            else
            {

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(rad);
            }
        }
        /// <summary>
        /// Ažuriranje vrste rada
        /// </summary>
        /// <param name="id">id vrste rada koja se ažurira</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            //za različite mogućnosti ažuriranja pogledati
            //attach, update, samo id, ...
            //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud#update-the-edit-page

            try
            {
                VrstaRada rad = await ctx.VrstaRada
                                  .Where(d => d.Id == id)
                                  .FirstOrDefaultAsync();
                if (rad == null)
                {
                    return NotFound("Neispravan id rada: " + id);
                }

                if (await TryUpdateModelAsync<VrstaRada>(rad, "", d => d.VrstaRada1))
                {

                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        ctx.SaveChanges();
                        TempData[Constants.Message] = "Vrsta rada ažurirana";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View("Edit", rad);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o vrsti rada nije moguće povezati s forme");
                    return View("Edit", rad);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Update), id);
            }
        }
        /// <summary>
        /// Brisanje vrste rada
        /// </summary>
        /// <param name="Id">id vrste rada koja se briše</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        public IActionResult Delete(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var rad = ctx.VrstaRada.Find(Id);
            if (rad != null)
            {
                try
                {
                    string opis = rad.VrstaRada1;
                    ctx.Remove(rad);
                    ctx.SaveChanges();
                    logger.LogInformation($"Vrsta rada {opis} uspješno obrisana");
                    TempData[Constants.Message] = $"Vrsta rada {opis} uspješno obrisana";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja rada: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja rada: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji rad s oznakom: {0} ", Id);
                TempData[Constants.Message] = "Ne postoji rad s oznakom: " + Id;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });

        }
    }
}
