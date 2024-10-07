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
    /// Klasa koja predstavlja upravljač za mode evidencije rada
    /// </summary>
    public class EvidencijaRadaController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly AppSettings appData;
        private readonly ILogger<EvidencijaRadaController> logger;
        /// <summary>
        /// Konstruktor za upravljač evidencije rada
        /// </summary>
        /// <param name="ctx">kontekst za spajanje na bazu</param>
        /// <param name="options">postavke iz appsettings</param>
        /// <param name="logger">logger</param>
        public EvidencijaRadaController(Rppp04Context ctx,
        IOptionsSnapshot<AppSettings> options, ILogger<EvidencijaRadaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;

        }
        /// <summary>
        /// Funkcija koja se poziva za prikazivanje tablice evidencija
        /// </summary>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns>view sa popunjenim modelom evidencija za prikaz</returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;
            var query = ctx.EvidencijaRada
                .AsNoTracking();
            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji niti jedna evidencija");
                TempData[Constants.Message] = "Ne postoji niti jedna evidencija.";
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
            var evidencije = query
                  .Select(m => new EvidencijaRadaViewModel
                  {
                      Id = m.Id,
                      BrojSati = m.BrojSati,
                      OpisRada = m.OpisRada,
                      OpisZadatak = m.Zadatak.OpisZadatak,
                      VrstaRada = m.VrstaRada.VrstaRada1,
                      Email = m.Suradnik.Email,
             
                  }
                   )
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToList();
            var model = new EvidencijeRadaViewModel
            {
                Evidencije = evidencije,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        /// <summary>
        /// Funkcija za prikaz forme za dodavanje
        /// </summary>
        /// <returns>vraća jednostavnu formu za popunjavanje podataka o evidenciji</returns>
        [HttpGet]
        public async  Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }
        
        /// <summary>
        /// Funkcija koja obavlja dodavanje evidencije nakon popunjenih podataka
        /// </summary>
        /// <param name="evidencija">novostvorena evidencija</param>
        /// <returns>Vraća natrag na listu evidencija</returns>
        [HttpPost]
        public IActionResult Create(EvidencijaRada evidencija)
        {
            logger.LogTrace(JsonSerializer.Serialize(evidencija));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(evidencija);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Nova evidencija dodana.");

                    TempData[Constants.Message] = $"Nova evidencija dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove evidencije: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(evidencija);
                }
            }
            else
            {
                return View(evidencija);
            }
        }
        /// <summary>
        /// Otvaranje forme za mijenjanje podataka o evidenciji
        /// </summary>
        /// <param name="id">Id evidencije koja će se mijenjati</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns>View sa podatcima evidencije koja se onda može mijenjati</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var evidencija = ctx.EvidencijaRada.AsNoTracking().Where(d => d.Id == id).SingleOrDefault();
            if (evidencija == null)
            {
                logger.LogWarning("Ne postoji evidencija s oznakom: {0} ", id);
                return NotFound("Ne postoji evidencija s oznakom: " + id);
            }
            else
            {
                await PrepareDropDownLists();
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(evidencija);
            }
        }
        /// <summary>
        /// Metoda koja obavlja spremanje izmijenjenih podataka o evidenciji
        /// </summary>
        /// <param name="id">Id evidencije koja će se mijenjati</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns>view za prikaz liste evidencija</returns>

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            //za različite mogućnosti ažuriranja pogledati
            //attach, update, samo id, ...
            //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud#update-the-edit-page

            try
            {
                EvidencijaRada evidencija = await ctx.EvidencijaRada
                                  .Where(d => d.Id == id)
                                  .FirstOrDefaultAsync();
                if (evidencija == null)
                {
                    return NotFound("Neispravan id evidencije: " + id);
                }

                if (await TryUpdateModelAsync<EvidencijaRada>(evidencija, "",d => d.BrojSati,
                    d => d.VrstaRadaId, d => d.ZadatakId, d => d.VrstaRadaId, d => d.SuradnikId))
                {

                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        ctx.SaveChanges();
                        string opis = evidencija.OpisRada;
                        logger.LogInformation(new EventId(1000), $"Evidencija s opisom {opis} ažurirana.");
                        TempData[Constants.Message] = "Evidencija ažurirana.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View("Edit", evidencija);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o evidenciji nije moguće povezati s forme");
                    return View("Edit", evidencija);
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
        /// metoda koja obavlja brisanje evidencije
        /// </summary>
        /// <param name="Id">Id evidencije koja će se obrisati</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        public IActionResult Delete(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var evidencija = ctx.EvidencijaRada.Find(Id);
            if (evidencija != null)
            {
                try
                {
                    string opis = evidencija.OpisRada;
                    ctx.Remove(evidencija);
                    ctx.SaveChanges();
                    logger.LogInformation($"Evidencija s opisom {opis} uspješno obrisana");
                    TempData[Constants.Message] = $"Evidencija s opisom {opis} uspješno obrisana";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja evidencije: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja evidencije: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji evidencija s oznakom: {0} ", Id);
                TempData[Constants.Message] = "Ne postoji evidencija s oznakom: " + Id;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });

        }
        /// <summary>
        /// Pomoćna funkcija koja puni ViewBag sa podacima za padajuće liste
        /// </summary>
        /// <returns></returns>
        private async Task PrepareDropDownLists()
        {
            var suradnici = await ctx.Suradnik.OrderBy(d => d.Email)
            .Select(d => new { d.Email, d.Id })
            .ToListAsync();
            ViewBag.Suradnici = new SelectList(suradnici,
            nameof(Suradnik.Id), nameof(Suradnik.Email));

            var vrsterada = await ctx.VrstaRada.OrderBy(d => d.Id)
                .Select(d => new { d.VrstaRada1, d.Id })
                .ToListAsync();
            ViewBag.VrsteRada = new SelectList(vrsterada,
            nameof(VrstaRada.Id), nameof(VrstaRada.VrstaRada1));

            var zadatci = await ctx.Zadatak.OrderBy(d => d.Id)
                .Select(d => new { d.OpisZadatak, d.Id })
                .ToListAsync();
            ViewBag.Zadatci = new SelectList(zadatci,
            nameof(Zadatak.Id), nameof(Zadatak.OpisZadatak));
        }
    }
}