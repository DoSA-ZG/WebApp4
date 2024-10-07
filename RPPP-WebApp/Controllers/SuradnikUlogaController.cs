using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers;

/// <summary>
/// Kontroler za upravljanje ulogama suradnika na projektima
/// </summary>
public class SuradnikUlogaController : Controller
{
    private readonly Rppp04Context ctx;
    private readonly ILogger<SuradnikUlogaController> logger;
    private readonly AppSettings appData;

    public SuradnikUlogaController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options,
        ILogger<SuradnikUlogaController> logger)
    {
        this.ctx = ctx;
        this.logger = logger;
        appData = options.Value;
    }

    /// <summary>
    /// Prikazuje listu uloga suradnika s opcijama za straničenje i sortiranje.
    /// </summary>
    /// <param name="page">Broj stranice.</param>
    /// <param name="sort">Vrsta sortiranja.</param>
    /// <param name="ascending">Smjer sortiranja.</param>
    /// <returns>Prikazuje listu suradnika uloga.</returns>
    public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appData.PageSize;

        var query = ctx.SuradnikUloga.AsQueryable();
        int count = await query.CountAsync();

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
            return RedirectToAction(nameof(Index), new { page = 1, sort = sort, ascending = ascending });
        }

        query = query.ApplySort(sort, ascending);

        var suradnikUloge = await query
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .Include(suradnikUloga => suradnikUloga.Projekt)
            .Include(suradnikUloga => suradnikUloga.Suradnik)
            .Include(suradnikUloga => suradnikUloga.VrstaUloge)
            .Select(suradnikUloga => SuradnikUlogaViewModelMapper(suradnikUloga))
            .ToListAsync();
        var model = new SuradnikUlogeViewModel()
        {
            SuradnikUloge = suradnikUloge,
            PagingInfo = pagingInfo
        };

        return View(model);
    }

    /// <summary>
    /// Prikazuje formu za dodavanje nove uloge suradnika.
    /// </summary>
    /// <returns>Prikazuje formu za unos nove uloge suradnika.</returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PrepareDropDownLists();
        return View();
    }

    /// <summary>
    /// Dodaje novu ulogu suradnika u bazu podataka.
    /// </summary>
    /// <param name="model">Podaci o ulozi suradnika na određenom projektu.</param>
    /// <returns>Preusmjerava na akciju Index nakon dodavanja.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SuradnikUloga model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                ctx.Add(model);
                await ctx.SaveChangesAsync();
                logger.LogInformation(new EventId(1030), $"Suradnik uloga {model.Id} dodana.");
                TempData[Constants.Message] = $"Uloga suradnika uspješno dodana. Id={model.Id}";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom dodavanje nove suradnik uloge: {0}",
                    exc.CompleteExceptionMessage());
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                await PrepareDropDownLists();
                return View(model);
            }
        }
        else
        {
            await PrepareDropDownLists();
            return View(model);
        }
    }

    /// <summary>
    /// Briše ulogu suradnika s određenim ID-jem
    /// </summary>
    /// <param name="id">ID uloge suradnika</param>
    /// <returns>Poruku za obavještavanje korisnika o ishodu brisanja.</returns>
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        ActionResponseMessage responseMessage;
        var suradnikUloga = await ctx.SuradnikUloga.FindAsync(id);
        if (suradnikUloga != null)
        {
            try
            {
                ctx.Remove(suradnikUloga);
                await ctx.SaveChangesAsync();
                responseMessage =
                    new ActionResponseMessage(MessageType.Success, $"Uloga suradnika uspješno obrisana.");
            }
            catch (Exception exc)
            {
                responseMessage = new ActionResponseMessage(MessageType.Error,
                    "Pogreška prilikom brisanja uloge suradnika: " + exc.CompleteExceptionMessage());
            }
        }
        else
        {
            responseMessage = new ActionResponseMessage(MessageType.Error, "Ne postoji uloga suradnika koja ima ID " + id);
        }

        Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
        return responseMessage.MessageType == MessageType.Success ? new EmptyResult() : await Get(id);
    }

    /// <summary>
    /// Prikazuje formu za uređivanje postojeće uloge suradnika.
    /// </summary>
    /// <param name="id">ID uloge suradnika</param>
    /// <param name="page">Broj stranice</param>
    /// <param name="sort">Kriterij po kojem se sortira</param>
    /// <param name="ascending">Smjer sortiranja</param>
    /// <returns>Forma za uređivanje uloge suradnika.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        var suradnikUloga = await ctx.SuradnikUloga.FindAsync(id);
        if (suradnikUloga == null)
        {
            return NotFound("Ne postoji suradnik uloga s traženim ID-om: " + id);
        }

        await PrepareDropDownLists();
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        return View(suradnikUloga);
    }

    /// <summary>
    /// Potvrđuje izmjenu uloge suradnika i sprema promjene u bazu podataka.
    /// </summary>
    /// <param name="model">Izmijenjeni podaci za ulogu suradnika</param>
    /// <param name="page">Broj stranice</param>
    /// <param name="sort">Kriterij po kojem se sortira</param>
    /// <param name="ascending">Smjer sortiranja</param>
    /// <returns>Poruku o ishodu i preusmjeravanje na Index.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SuradnikUloga model, int page = 1, int sort = 1, bool ascending = true)
    {
        if (model == null)
        {
            return NotFound("Nema poslanih podataka");
        }

        var suradnikUloga = await ctx.SuradnikUloga.FindAsync(model.Id);
        if (suradnikUloga == null)
        {
            return NotFound("Ne postoji suradnik uloga s navedenim ID-om " + suradnikUloga.Id);
        }

        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;

        if (ModelState.IsValid)
        {
            try
            {
                CopyValues(model, suradnikUloga);
                ctx.Update(suradnikUloga);
                await ctx.SaveChangesAsync();
                logger.LogInformation($"Suradnik uloga: {model.Id} uspješno ažurirana");
                TempData[Constants.Message] = $"Suradnik uloga {model.Id} uspješno ažurirana";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                return View(model);
            }
        }
        else
        {
            return View(model);
        }
    }

    /// <summary>
    /// Dohvaća djelomični prikaz s podacima o jednoj ulozi suradnika.
    /// </summary>
    /// <param name="id">ID uloge suradnika</param>
    /// <returns>PartialView prikaz</returns>
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var suradnikUloga = await ctx.SuradnikUloga.FindAsync(id);
        if (suradnikUloga != null)
        {
            return PartialView(SuradnikUlogaViewModelMapper(suradnikUloga));
        }

        return NotFound($"Ne postoji suradnik uloga s traženim ID-om: " + id);
    }

    #region Private methods

    private async Task PrepareDropDownLists()
    {
        var projekti = await ctx.Projekt
            .Select(p => new { p.Id, p.NazivProjekt })
            .ToListAsync();
        ViewBag.Projekti =
            new SelectList(projekti, nameof(Projekt.Id), nameof(Projekt.NazivProjekt));
        var vrsteUloga = await ctx.VrstaUloge
            .Select(p => new { p.Id, p.Vrsta })
            .ToListAsync();
        ViewBag.VrsteUloga =
            new SelectList(vrsteUloga, nameof(VrstaUloge.Id), nameof(VrstaUloge.Vrsta));
        
        var suradnici = await ctx.Suradnik
            .Select(p => new { p.Id, p.Ime, p.Prezime })
            .ToListAsync();
        var selectListItems = suradnici.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = $"{s.Ime} {s.Prezime}"
        }).ToList();
        ViewBag.Suradnici = new SelectList(selectListItems, "Value", "Text");
    }

    private static SuradnikUlogaViewModel SuradnikUlogaViewModelMapper(SuradnikUloga suradnikUloga)
    {
        return new SuradnikUlogaViewModel
        {
            Id = suradnikUloga.Id,
            DatumPocetak = suradnikUloga.DatumPocetak,
            DatumKraj = suradnikUloga.DatumKraj,
            Projekt = suradnikUloga.Projekt,
            Suradnik = suradnikUloga.Suradnik,
            VrstaUloge = suradnikUloga.VrstaUloge
        };
    }

    private void CopyValues(SuradnikUloga model, SuradnikUloga suradnikUloga)
    {
        suradnikUloga.Id = model.Id;
        suradnikUloga.DatumPocetak = model.DatumPocetak;
        suradnikUloga.DatumKraj = model.DatumKraj;
        suradnikUloga.ProjektId = model.ProjektId;
        suradnikUloga.SuradnikId = model.SuradnikId;
        suradnikUloga.VrstaUlogeId = model.VrstaUlogeId;
    }

    #endregion
}