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
/// Kontroler za upravljanje vrstama suradnika
/// </summary>
public class VrstaSuradnikaController : Controller
{
    private readonly Rppp04Context ctx;
    private readonly ILogger<VrstaSuradnikaController> logger;
    private readonly AppSettings appData;

    public VrstaSuradnikaController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options,
        ILogger<VrstaSuradnikaController> logger)
    {
        this.ctx = ctx;
        this.logger = logger;
        appData = options.Value;
    }

    /// <summary>
    /// Prikazuje listu vrsta suradnika s opcijama za straničenje i sortiranje.
    /// </summary>
    /// <param name="page">Broj stranice.</param>
    /// <param name="sort">Vrsta sortiranja.</param>
    /// <param name="ascending">Smjer sortiranja.</param>
    /// <returns>Prikazuje listu vrsta suradnika.</returns>
    public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appData.PageSize;

        var query = ctx.VrstaSuradnika.AsQueryable();
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

        var vrsteSuradnika = await query
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .Select(v => VrstaSuradnikaViewModelMapper(v))
            .ToListAsync();
        var model = new VrsteSuradnikaViewModel()
        {
            VrsteSuradnika = vrsteSuradnika,
            PagingInfo = pagingInfo
        };

        return View(model);
    }

    /// <summary>
    /// Prikazuje formu za dodavanje nove vrste suradnika.
    /// </summary>
    /// <returns>Prikazuje formu za unos novog korisničkog računa.</returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View();
    }

    /// <summary>
    /// Dodaje novu vrstu suradnika u bazu podataka.
    /// </summary>
    /// <param name="model">Podaci o vrsti suradnika.</param>
    /// <returns>Preusmjerava na akciju Index nakon dodavanja.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VrstaSuradnika model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                ctx.Add(model);
                await ctx.SaveChangesAsync();
                logger.LogInformation(new EventId(1111), $"Vrsta suradnika {model.Id} dodana.");
                TempData[Constants.Message] = $"Vrsta suradnika '{model.Vrsta}' uspješno dodana.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom dodavanje nove vrste suradnika: {0}",
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
    /// Briše vrstu suradnika s određenim ID-jem
    /// </summary>
    /// <param name="id">ID vrste suradnika</param>
    /// <returns>Poruku za obavještavanje korisnika o ishodu brisanja.</returns>
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        ActionResponseMessage responseMessage;
        var vrstaSuradnika = await ctx.VrstaSuradnika.FindAsync(id);
        if (vrstaSuradnika != null)
        {
            try
            {
                ctx.Remove(vrstaSuradnika);
                await ctx.SaveChangesAsync();
                responseMessage =
                    new ActionResponseMessage(MessageType.Success, $"Vrsta suradnika uspješno obrisana.");
            }
            catch (Exception exc)
            {
                responseMessage = new ActionResponseMessage(MessageType.Error,
                    "Pogreška prilikom brisanja vrste suradnika: " + exc.CompleteExceptionMessage());
            }
        }
        else
        {
            responseMessage = new ActionResponseMessage(MessageType.Error, "Ne postoji vrsta suradnika koja ima ID " + id);
        }

        Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
        return responseMessage.MessageType == MessageType.Success ? new EmptyResult() : await Get(id);
    }

    /// <summary>
    /// Prikazuje formu za uređivanje postojeće vrste suradnika.
    /// </summary>
    /// <param name="id">ID vrste suradnika</param>
    /// <param name="page">Broj stranice</param>
    /// <param name="sort">Kriterij po kojem se sortira</param>
    /// <param name="ascending">Smjer sortiranja</param>
    /// <returns>Forma za uređivanje vrste suradnika.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        var vrstaSuradnika = await ctx.VrstaSuradnika.FindAsync(id);
        if (vrstaSuradnika == null)
        {
            return NotFound("Ne postoji vrsta suradnika s traženim ID-om: " + id);
        }

        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        return View(vrstaSuradnika);
    }

    /// <summary>
    /// Potvrđuje izmjenu vrste suradnika i sprema promjene u bazu podataka.
    /// </summary>
    /// <param name="model">Izmijenjeni podaci za vrstu suradnika</param>
    /// <param name="page">Broj stranice</param>
    /// <param name="sort">Kriterij po kojem se sortira</param>
    /// <param name="ascending">Smjer sortiranja</param>
    /// <returns>Poruku o ishodu i preusmjeravanje na Index.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VrstaSuradnika model, int page = 1, int sort = 1, bool ascending = true)
    {
        if (model == null)
        {
            return NotFound("Nema poslanih podataka");
        }

        var vrstaSuradnika = await ctx.VrstaSuradnika.FindAsync(model.Id);
        if (vrstaSuradnika == null)
        {
            return NotFound("Ne postoji vrsta suradnika s navedenim ID-om " + vrstaSuradnika.Id);
        }

        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;

        if (ModelState.IsValid)
        {
            try
            {
                CopyValues(model, vrstaSuradnika);
                ctx.Update(vrstaSuradnika);
                await ctx.SaveChangesAsync();
                logger.LogInformation($"Vrsta suradnika: {model.Id} uspješno ažurirana");
                TempData[Constants.Message] = $"Vrsta suradnika {model.Id} uspješno ažurirana";
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
    /// Dohvaća djelomični prikaz s podacima o jednoj vrsti suradnika.
    /// </summary>
    /// <param name="id">ID vrste suradnika</param>
    /// <returns>PartialView prikaz</returns>
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var vrstaSuradnika = await ctx.VrstaSuradnika.FindAsync(id);
        if (vrstaSuradnika != null)
        {
            return PartialView(VrstaSuradnikaViewModelMapper(vrstaSuradnika));
        }

        return NotFound($"Ne postoji vrsta suradnika s traženim ID-om: " + id);
    }

    #region Private methods

    private static VrstaSuradnikaViewModel VrstaSuradnikaViewModelMapper(VrstaSuradnika vrstaSuradnika)
    {
        return new VrstaSuradnikaViewModel
        {
            Id = vrstaSuradnika.Id,
            Vrsta = vrstaSuradnika.Vrsta
        };
    }

    private void CopyValues(VrstaSuradnika model, VrstaSuradnika vrstaSuradnika)
    {
        vrstaSuradnika.Id = model.Id;
        vrstaSuradnika.Vrsta = model.Vrsta;
    }

    #endregion
}