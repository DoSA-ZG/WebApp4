using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Filters;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Views.ProjektniZahtjevMD;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Klasa koja predstavlja upravljač pri radu sa zadatcima
    /// </summary>
    public class ZadatakController : Controller
    {
        private readonly Rppp04Context ctx;
        private readonly AppSettings appData;
        private readonly ILogger<ZadatakController> logger;
        /// <summary>
        /// Konstruktor za zadatakcontroller
        /// </summary>
        /// <param name="ctx">kontekst za spajanje na bazu</param>
        /// <param name="options">postavke iz appsettings</param>
        /// <param name="logger">logger</param>
        public ZadatakController(Rppp04Context ctx,
        IOptionsSnapshot<AppSettings> options, ILogger<ZadatakController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
            
        }
        /// <summary>
        /// Funkcija koja se poziva za prikazivanje tablice zadataka
        /// </summary>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns>View sa popunjenom tablicom zadataka</returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;
            var query = ctx.vw_Zadatak.AsQueryable();
            int count = query.Count();
            if(count == 0)
            {
                logger.LogInformation("Ne postoji niti jedan zadatak");
                TempData[Constants.Message] = "Ne postoji niti jedan zadatak.";
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
            var zadatci =  query
                 /* .Select(m => new ZadatakViewModel
                  {
                      Id = m.Id,
                      OpisZadatak = m.OpisZadatak,
                      PlanPocetak = m.PlanPocetak,
                      PlanKraj = m.PlanKraj,
                      StvarniPocetak = m.StvarniPocetak,
                      StvarniKraj = m.StvarniKraj,
                      Email = m.Email,
                      NazivPrioriteta = m.NazivPrioriteta,
                      Status = m.Status
                  }
                   )*/
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToList();
            for (int i = 0; i < zadatci.Count; i++)
            {
                zadatci[i].Position = (page - 1) * pagesize + i;
            }
            var model = new ZadatciViewModel
            {
                Zadatci = zadatci,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        /// <summary>
        /// Funkcija koja obavlja ažuriranje zadatka unutar MD forme
        /// </summary>
        /// <param name="id">id zadatka koji se ažurira</param>
        /// <param name="position">pozicija zadatka u tablici zadataka</param>
        /// <param name="page">stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Change(ZadatakViewModel zad, int id, int? position, int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            ViewBag.Position = position;
            if (ModelState.IsValid)
            {
                try
                {
                    Zadatak zadatak = await ctx.Zadatak
                                      .Where(d => d.Id == id)
                                      .FirstOrDefaultAsync();
                    if (zadatak == null)
                    {
                        return NotFound("Neispravan id zadatka: " + id);
                    }
                    zadatak.OpisZadatak = zad.OpisZadatak;
                    zadatak.StvarniKraj = zad.StvarniKraj;
                    zadatak.StvarniPocetak = zad.StvarniPocetak;
                    zadatak.PlanPocetak = zad.PlanPocetak;
                    zadatak.PlanKraj= zad.PlanKraj;
                    zadatak.SuradnikId = zad.SuradnikId;
                    zadatak.ProjektniZahtjevId = zad.ProjektniZahtjevId;
                    zadatak.PrioritetZadatkaId = zad.PrioritetZadatkaId;
                    zadatak.StatusZadatkaId = zad.StatusZadatkaId;
                    if (ctx.SaveChanges() >0)
                    {
                        try
                        {
                            
                            await PrepareDropDownLists();
                            logger.LogInformation($"zadatak s id {id} uspješno ažuriran");
                            TempData[Constants.Message] = "Zadatak ažuriran.";
                            TempData[Constants.ErrorOccurred] = false;
                            return RedirectToAction(nameof(Show), new { id = id, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
                        }
                        catch (Exception exc)
                        {
                            await PrepareDropDownLists();
                            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                            return RedirectToAction(nameof(Show), new { id = id, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
                        }
                    }
                    else
                    {
                        await PrepareDropDownLists();
                        ModelState.AddModelError(string.Empty, "Podatke o zadatku nije moguće povezati s forme");
                        return RedirectToAction(nameof(Show), new { id = id, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
                    }

                   
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    await PrepareDropDownLists();
                    return RedirectToAction(nameof(Show), new { id = id, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
                }

            }
            else
            {
                return RedirectToAction(nameof(Show), new { id = id, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
            }

        }
        /// <summary>
        /// Funkcija koja briše evidenciju unutar MD forme
        /// </summary>
        /// <param name="Id">id evidencije koja se briše</param>
        /// <param name="position">pozicija zadatka</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        public IActionResult DeleteEv(int Id, int? position, int page = 1, int sort = 1, bool ascending = true)
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
                    return RedirectToAction(nameof(Show), new { id = evidencija.ZadatakId, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
                }
            }
            else
            {
                logger.LogWarning("Ne postoji evidencija s oznakom: {0} ", Id);
                TempData[Constants.Message] = "Ne postoji evidencija s oznakom: " + Id;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Show), new { id = evidencija.ZadatakId, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });


        }
        /// <summary>
        /// Funkcija koja obavlja stvaranje evidencije unutar MD forme
        /// </summary>
        /// <param name="evidencija">evidencija koja se dodaje</param>
        /// <param name="zadid">id zadatka kojem evidencija pripada</param>
        /// <param name="position">pozicija zadatka</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateEv(EvidencijaRada evidencija , int zadid, int? position, int page = 1, int sort = 1, bool ascending = true)
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
                    return RedirectToAction(nameof(Show), new { id = zadid, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove evidencije: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return RedirectToAction(nameof(Show), new { id = zadid, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
                }
            }
            else
            {
                return RedirectToAction(nameof(Show), new { id = zadid, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
            }
        }

        /// <summary>
        /// Metoda koja ažurira pojedinu evidenciju unutar složene MD forme
        /// </summary>
        /// <param name="zadid">id zadatka kojem evidencija pripada</param>
        /// <param name="id">id evidencije koja se ažurira</param>
        /// <param name="position">pozicija zadatka u tablici zadataka</param>
        /// <param name="page">stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>

        [HttpPost, ActionName("EditEv")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSingleEv(int zadid, int id, int? position, int page = 1, int sort = 1, bool ascending = true)
        {
            //za različite mogućnosti ažuriranja pogledati
            //attach, update, samo id, ...
            //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud#update-the-edit-page
            if (ModelState.IsValid) { 
            try
            {
                EvidencijaRada evidencija = await ctx.EvidencijaRada
                                  .Where(d => d.Id == id)
                                  .FirstOrDefaultAsync();
                if (evidencija == null)
                {
                    return NotFound("Neispravan id evidencije: " + id);
                }

                if (await TryUpdateModelAsync<EvidencijaRada>(evidencija, "", d => d.BrojSati, d => d.OpisRada,
                    d => d.VrstaRadaId, d => d.ZadatakId, d => d.VrstaRadaId, d => d.SuradnikId))
                {

                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        ctx.SaveChanges();
                        TempData[Constants.Message] = "Evidencija ažurirana.";
                        logger.LogInformation($"Evidencija s id: {id} ažurirana");

                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Show), new { id = zadid, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return RedirectToAction(nameof(Show), new { id = zadid, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o evidenciji nije moguće povezati s forme");
                    return RedirectToAction(nameof(Show), new { id = zadid, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Change), zadid);
            }
            }
            else
            {
                return RedirectToAction(nameof(Show), new { id = zadid, position = position, page = page, sort = sort, ascending = ascending, viewName = nameof(Change) });


            }
        }
        /// <summary>
        /// Ovisno o vrijednosti parametra viewName prikazuje ili MD formu ili obični prikaz MD podataka zadatka
        /// </summary>
        /// <param name="id">id zadatka koji će se prikazati</param>
        /// <param name="position">pozicija trenutnog zadatka</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <param name="viewName">ime viewa-a koje će se prikazati</param>
        /// <returns></returns>
        public async Task<IActionResult> Show(int id, int? position, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Show))
        {


            await PrepareDropDownLists();
            var zadatak = await ctx.Zadatak
                                    .Where(d => d.Id == id)
                                    .Select(d => new ZadatakViewModel
                                    {
                                        Id = d.Id,
                                        OpisZadatak = d.OpisZadatak,
                                        PlanPocetak = d.PlanPocetak,
                                        PlanKraj = d.PlanKraj,
                                        StvarniPocetak = d.StvarniPocetak,
                                        StvarniKraj = d.StvarniKraj,
                                        Email = d.Suradnik.Email,
                                        NazivPrioriteta = d.PrioritetZadatka.NazivPrioriteta,
                                        Status = d.StatusZadatka.Status,
                                        ProjektniZahtjevId = d.ProjektniZahtjev.Id,
                                        StatusZadatkaId = d.StatusZadatka.Id,
                                        SuradnikId= d.SuradnikId,
                                        PrioritetZadatkaId = d.PrioritetZadatka.Id
                                    })
                                    .FirstOrDefaultAsync();
            if (zadatak == null)
            {
                return NotFound($"Zadatak {id} ne postoji");
            }
            else
            {
            
                //učitavanje evidencija
                var evidencije = await ctx.EvidencijaRada
                                      .Where(s => s.ZadatakId == zadatak.Id)
                                      .OrderBy(s => s.Id)
                                      .Select(s => new EvidencijaRadaViewModel
                                      {
                                          Id = s.Id,
                                          BrojSati = s.BrojSati,
                                          OpisRada = s.OpisRada,
                                          OpisZadatak =s.Zadatak.OpisZadatak,
                                          VrstaRada = s.VrstaRada.VrstaRada1,
                                          Email = s.Suradnik.Email,
                                          SuradnikId = s.SuradnikId,
                                          VrstaRadaId = s.VrstaRadaId,
                                          ZadatakId = s.ZadatakId
                                      })
                                      .ToListAsync();
                zadatak.Evidencije = evidencije;

                if (position.HasValue)
                {
                    page = 1 + position.Value / appData.PageSize;
                    await SetPreviousAndNext(position.Value,sort, ascending);
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Position = position;

                return View(viewName, zadatak);
            }
        }
        /// <summary>
        /// Postavlja previous i next zadatka da bi omogućilo šetanje po zadatcima
        /// </summary>
        /// <param name="position">pozicija trenutnog zadatka</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        private async Task SetPreviousAndNext(int position,int sort, bool ascending)
        {
            var query = ctx.vw_Zadatak.AsQueryable();
        

            query = query.ApplySort(sort, ascending);
            if (position > 0)
            {
                ViewBag.Previous = await query.Skip(position - 1).Select(d => d.Id).FirstAsync();
            }
            if (position < await query.CountAsync() - 1)
            {
                ViewBag.Next = await query.Skip(position + 1).Select(d => d.Id).FirstAsync();
            }
        }

        /// <summary>
        /// Funkcija za prikaz forme za stvaranje zadatka
        /// </summary>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create(int page = 1, int sort = 1, bool ascending = true)
        {
            TempData["page"] = page;
            TempData["sort"]= sort;
            TempData["ascending"] = ascending;
            await PrepareDropDownLists();
            return View();
        }

        /// <summary>
        /// Funkcija koja obavlja stvaranje novog zadatka
        /// </summary>
        /// <param name="zadatak">novi zadatak</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(Zadatak zadatak, int page, int sort, bool ascending)
        {
            logger.LogTrace(JsonSerializer.Serialize(zadatak));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(zadatak);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Novi zadatak dodan.");

                    TempData[Constants.Message] = "Novi zadatak dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja novog zadatka: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(zadatak);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return View(zadatak);
            }
        }



        /// <summary>
        /// obavlja brisanje zadatka
        /// </summary>
        /// <param name="Id">Id zadatka koji se briše</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Delete(int Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var zadatak = ctx.Zadatak.Find(Id);
            if (zadatak != null)
            {
                try
                {
                    string naziv = zadatak.OpisZadatak;
                    ctx.Remove(zadatak);
                    ctx.SaveChanges();
                    logger.LogInformation($"Zadatak {naziv} uspješno obrisan");
                    TempData[Constants.Message] = $"Zadatak {naziv} uspješno obrisan";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja zadatka: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja zadatka: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji zadatak s oznakom: {0} ", Id);
                TempData[Constants.Message] = "Ne postoji zadatak s oznakom: " + Id;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });

        }
        /// <summary>
        /// Otvara master-detail prikaz zadatka i njegovih evidencija u kojem je omogućeno dodavanje/brisanje/mijenjanje podataka vezanih uz zadatak i evidencije
        /// </summary>
        /// <param name="id">Id zadatka koji će se prikazivati</param>
        /// <param name="position">pozicija zadatka u tablici zadataka</param>
        /// <param name="page">trenutna stranica</param>
        /// <param name="sort">stupac po kojem se sortira</param>
        /// <param name="ascending">da li je sortiranje uzlazno ili silazno</param>
        /// <returns></returns>
        [HttpGet  ,ActionName("Change")]
        public async Task<IActionResult> ChangeGet(int id, int? position, int page = 1, int sort = 1, bool ascending = true)
        {
            return await Show(id,position, page, sort, ascending, viewName: nameof(Change));

        }
        /// <summary>
        /// Pomoćna funkcija koja puni ViewBag sa podacima za padajuće liste
        /// </summary>
        /// <returns></returns>
        private async Task  PrepareDropDownLists()
        {
            var suradnici = await ctx.Suradnik.OrderBy(d => d.Email)
            .Select(d => new { d.Email, d.Id })
            .ToListAsync();
            ViewBag.Suradnici = new SelectList(suradnici,
            nameof(Suradnik.Id), nameof(Suradnik.Email));

            var statusi = await ctx.StatusZadatka.OrderBy(d => d.Id)
            .Select(d => new { d.Status, d.Id })
            .ToListAsync();
            ViewBag.Statusi = new SelectList(statusi,
            nameof(StatusZadatka.Id), nameof(StatusZadatka.Status));

            var prioriteti = await ctx.PrioritetZadatka.OrderBy(d => d.Id)
            .Select(d => new { d.NazivPrioriteta, d.Id })
            .ToListAsync();
            ViewBag.Prioriteti = new SelectList(prioriteti,
            nameof(PrioritetZadatka.Id), nameof(PrioritetZadatka.NazivPrioriteta));

            var zahtjevi = await ctx.ProjektniZahtjev.OrderBy(d => d.Id)
            .Select(d => new { d.OpisZahtjeva, d.Id })
            .ToListAsync();
            ViewBag.Zahtjevi = new SelectList(zahtjevi,
            nameof(ProjektniZahtjev.Id), nameof(ProjektniZahtjev.OpisZahtjeva));

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