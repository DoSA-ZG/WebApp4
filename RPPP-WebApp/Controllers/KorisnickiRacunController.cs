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
/// Kontroler za upravljanje korisničkim računima
/// </summary>
public class KorisnickiRacunController : Controller
{
    private readonly Rppp04Context ctx;
    private readonly ILogger<KorisnickiRacunController> logger;
    private readonly AppSettings appData;

    public KorisnickiRacunController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options,
        ILogger<KorisnickiRacunController> logger)
    {
        this.ctx = ctx;
        this.logger = logger;
        appData = options.Value;
    }

    /// <summary>
    /// Prikazuje listu korisničkih računa s opcijama za straničenje i sortiranje.
    /// </summary>
    /// <param name="page">Broj stranice.</param>
    /// <param name="sort">Vrsta sortiranja.</param>
    /// <param name="ascending">Smjer sortiranja.</param>
    /// <returns>Prikazuje listu korisničkih računa.</returns>
    public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appData.PageSize;

        var query = ctx.KorisnickiRacun.AsQueryable();
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

        var korisnickiRacuni = await query
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .Select(v => KorisnickiRacunViewModelMapper(v))
            .ToListAsync();
        var model = new KorisnickiRacuniViewModel()
        {
            KorisnickiRacuni = korisnickiRacuni,
            PagingInfo = pagingInfo
        };

        return View(model);
    }

    /// <summary>
    /// Prikazuje formu za dodavanje novog korisničkog računa.
    /// </summary>
    /// <returns>Prikazuje formu za unos novog korisničkog računa.</returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View();
    }

    /// <summary>
    /// Dodaje novi korisnički račun u bazu podataka.
    /// </summary>
    /// <param name="model">Podaci o korisničkom računu.</param>
    /// <returns>Preusmjerava na akciju Index nakon dodavanja.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KorisnickiRacun model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                ctx.Add(model);
                await ctx.SaveChangesAsync();
                logger.LogInformation(new EventId(1603), $"Korisnicki racun ID {model.Id} dodan.");
                TempData[Constants.Message] = $"Korisnicki racun ID {model.Id} uspješno dodan.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom dodavanje novog korisnickog racuna: {0}",
                    exc.CompleteExceptionMessage());
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
    /// Briše korisnički račun s određenim ID-jem
    /// </summary>
    /// <param name="id">ID korisničkog računa</param>
    /// <returns>Poruku za obavještavanje korisnika o ishodu brisanja.</returns>
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        ActionResponseMessage responseMessage;
        var korisnickiRacun = await ctx.KorisnickiRacun.FindAsync(id);
        if (korisnickiRacun != null)
        {
            try
            {
                ctx.Remove(korisnickiRacun);
                await ctx.SaveChangesAsync();
                responseMessage =
                    new ActionResponseMessage(MessageType.Success, $"Korisnički račun uspješno obrisan.");
            }
            catch (Exception exc)
            {
                responseMessage = new ActionResponseMessage(MessageType.Error,
                    "Pogreška prilikom brisanja vrste uloge: " + exc.CompleteExceptionMessage());
            }
        }
        else
        {
            responseMessage =
                new ActionResponseMessage(MessageType.Error, "Ne postoji korisnički račun koji ima ID " + id);
        }

        Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
        return responseMessage.MessageType == MessageType.Success ? new EmptyResult() : await Get(id);
    }

    /// <summary>
    /// Prikazuje formu za uređivanje postojećeg korisničkog računa.
    /// </summary>
    /// <param name="id">ID korisnićkog računa</param>
    /// <param name="page">Broj stranice</param>
    /// <param name="sort">Kriterij po kojem se sortira</param>
    /// <param name="ascending">Smjer sortiranja</param>
    /// <returns>Forma za uređivanje dokumenta.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        var korisnickiRacun = await ctx.KorisnickiRacun.FindAsync(id);
        if (korisnickiRacun == null)
        {
            return NotFound("Ne postoji korisnički račun s traženim ID-om: " + id);
        }

        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        return View(korisnickiRacun);
    }

    /// <summary>
    /// Potvrđuje izmjenu korisničkog računa i sprema promjene u bazu podataka.
    /// </summary>
    /// <param name="model">Izmijenjeni podaci za korisnički račun</param>
    /// <param name="page">Broj stranice</param>
    /// <param name="sort">Kriterij po kojem se sortira</param>
    /// <param name="ascending">Smjer sortiranja</param>
    /// <returns>Poruku o ishodu i preusmjeravanje na Index.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(KorisnickiRacun model, int page = 1, int sort = 1, bool ascending = true)
    {
        if (model == null)
        {
            return NotFound("Nema poslanih podataka");
        }

        var korisnickiRacun = await ctx.KorisnickiRacun.FindAsync(model.Id);
        if (korisnickiRacun == null)
        {
            return NotFound("Ne postoji korisnički račun s navedenim ID-om " + korisnickiRacun.Id);
        }

        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;

        if (ModelState.IsValid)
        {
            try
            {
                CopyValues(model, korisnickiRacun);
                ctx.Update(korisnickiRacun);
                await ctx.SaveChangesAsync();
                logger.LogInformation($"Korisnički račun ID {model.Id} uspješno ažuriran");
                TempData[Constants.Message] = "Korisnički račun ID {model.Id} uspješno ažuriran";
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
    /// Dohvaća djelomični prikaz s podacima o jednom korisničkom računu.
    /// </summary>
    /// <param name="id">ID korisničkog računa</param>
    /// <returns>PartialView prikaz</returns>
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var korisnickiRacun = await ctx.KorisnickiRacun.FindAsync(id);
        if (korisnickiRacun != null)
        {
            return PartialView(KorisnickiRacunViewModelMapper(korisnickiRacun));
        }

        return NotFound($"Ne postoji korisnički račun s traženim ID-om: " + id);
    }

    #region Private methods

    private static KorisnickiRacunViewModel KorisnickiRacunViewModelMapper(KorisnickiRacun korisnickiRacun)
    {
        return new KorisnickiRacunViewModel
        {
            Id = korisnickiRacun.Id,
            StupanjPrava = korisnickiRacun.StupanjPrava
        };
    }

    private void CopyValues(KorisnickiRacun model, KorisnickiRacun korisnickiRacun)
    {
        korisnickiRacun.Id = model.Id;
        korisnickiRacun.StupanjPrava = model.StupanjPrava;
    }

    #endregion
}