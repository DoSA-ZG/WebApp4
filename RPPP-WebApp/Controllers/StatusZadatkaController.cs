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
    /// Klasa za upravljač modela statusa zadataka
    /// </summary>
    public class StatusZadatkaController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly AppSettings appData;
        private readonly ILogger<StatusZadatkaController> logger;
        public StatusZadatkaController(Rppp04Context ctx,
        IOptionsSnapshot<AppSettings> options, ILogger<StatusZadatkaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;

        }

        /// <summary>
        /// Prikazivanje tablice statusa
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
                var query = ctx.StatusZadatka
                    .AsNoTracking();
                int count = query.Count();
                if (count == 0)
                {
                    logger.LogInformation("Ne postoji niti jedan status");
                    TempData[Constants.Message] = "Ne postoji niti jedan status.";
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
                var statusi = query
                      .Select(m => new StatusZadatka
                      {
                          Id = m.Id,
                          Status = m.Status,

                      }
                       )
                      .Skip((page - 1) * pagesize)
                      .Take(pagesize)
                      .ToList();
                var model = new StatusiZadatkaViewModel
                {
                    statusi = statusi,
                    PagingInfo = pagingInfo
                };
                return View(model);
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom prikaza statusa: {0}", exc.CompleteExceptionMessage());
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                return View();
            }
        }
        /// <summary>
        /// Prikaz forme za stvaranje novog statusa
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {
            
            return View();
        }

        /// <summary>
        /// Dodavanje novog statusa 
        /// </summary>
        /// <param name="statusAdd">status koji se dodaje</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Create(StatusZadatka statusAdd)
        {
            logger.LogTrace(JsonSerializer.Serialize(statusAdd));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(statusAdd);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Novi status zadatka dodan.");

                    TempData[Constants.Message] = $"Novi status dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja novog statusa: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(statusAdd);
                }
            }
            else
            {
                return View(statusAdd);
            }
        }

        /// <summary>
        /// Prikaz forme za ažuriranje statusa
        /// </summary
        /// <param name="id">id statusa koji se ažurira</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var status = ctx.StatusZadatka.AsNoTracking().Where(d => d.Id == id).SingleOrDefault();
            if (status == null)
            {
                logger.LogWarning("Ne postoji status s oznakom: {0} ", id);
                return NotFound("Ne postoji status s oznakom: " + id);
            }
            else
            {
                
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(status);
            }
        }
        /// <summary>
        /// Ažuriranje statusa
        /// </summary>
        /// <param name="id">id statusa koji se ažurira</param>
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
                StatusZadatka status = await ctx.StatusZadatka
                                  .Where(d => d.Id == id)
                                  .FirstOrDefaultAsync();
                if (status == null)
                {
                    return NotFound("Neispravan id statusa: " + id);
                }

                if (await TryUpdateModelAsync<StatusZadatka>(status, "", d => d.Status))
                {

                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        ctx.SaveChanges();
                        TempData[Constants.Message] = "Status ažuriran";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View("Edit", status);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o statusu nije moguće povezati s forme");
                    return View("Edit", status);
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
        /// Brisanje statusa
        /// </summary>
        /// <param name="Id">id statusa koji se briše</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        public IActionResult Delete(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var status = ctx.StatusZadatka.Find(Id);
            if (status != null)
            {
                try
                {
                    string opis = status.Status;
                    ctx.Remove(status);
                    ctx.SaveChanges();
                    logger.LogInformation($"Status s opisom {opis} uspješno obrisan");
                    TempData[Constants.Message] = $"Status s opisom {opis} uspješno obrisan";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja statusa: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja statusa: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji status s oznakom: {0} ", Id);
                TempData[Constants.Message] = "Ne postoji status s oznakom: " + Id;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });

        }
    }
}
