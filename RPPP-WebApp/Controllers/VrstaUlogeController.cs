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
/// Kontroler za upravljanje vrstama uloga
/// </summary>
public class VrstaUlogeController : Controller
{
    private readonly Rppp04Context ctx;
    private readonly ILogger<VrstaUlogeController> logger;
    private readonly AppSettings appData;

    public VrstaUlogeController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options,
        ILogger<VrstaUlogeController> logger)
    {
        this.ctx = ctx;
        this.logger = logger;
        appData = options.Value;
    }

    /// <summary>
    /// Prikazuje listu vrsta uloga s opcijama za straničenje i sortiranje.
    /// </summary>
    /// <param name="page">Broj stranice.</param>
    /// <param name="sort">Vrsta sortiranja.</param>
    /// <param name="ascending">Smjer sortiranja.</param>
    /// <returns>Prikazuje listu vrsta uloga.</returns>
    public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appData.PageSize;

        var query = ctx.VrstaUloge.AsQueryable();
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

        var vrsteUloga = await query
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .Select(v => VrstaUlogeViewModelMapper(v))
            .ToListAsync();
        var model = new VrsteUlogaViewModel()
        {
            VrsteUloga = vrsteUloga,
            PagingInfo = pagingInfo
        };

        return View(model);
    }


    /// <summary>
    /// Prikazuje formu za dodavanje nove vrste uloge.
    /// </summary>
    /// <returns>Prikazuje formu za unos nove vrste uloge.</returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View();
    }

    /// <summary>
    /// Dodaje novu vrstu uloge u bazu podataka.
    /// </summary>
    /// <param name="model">Podaci o vrsti uloge.</param>
    /// <returns>Preusmjerava na akciju Index nakon dodavanja.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VrstaUloge model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                ctx.Add(model);
                await ctx.SaveChangesAsync();
                logger.LogInformation(new EventId(1111), $"Vrsta uloge {model.Id} dodana.");
                TempData[Constants.Message] = $"Vrsta uloge '{model.Vrsta}' uspješno dodana.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom dodavanje nove vrste uloga: {0}",
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
    /// Briše vrstu uloge s određenim ID-jem
    /// </summary>
    /// <param name="id">ID vrste uloge</param>
    /// <returns>Poruku za obavještavanje korisnika o ishodu brisanja.</returns>
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        ActionResponseMessage responseMessage;
        var vrstaUloge = await ctx.VrstaUloge.FindAsync(id);
        if (vrstaUloge != null)
        {
            try
            {
                ctx.Remove(vrstaUloge);
                await ctx.SaveChangesAsync();
                responseMessage =
                    new ActionResponseMessage(MessageType.Success, $"Vrsta uloge uspješno obrisana.");
            }
            catch (Exception exc)
            {
                responseMessage = new ActionResponseMessage(MessageType.Error,
                    "Pogreška prilikom brisanja vrste uloge: " + exc.CompleteExceptionMessage());
            }
        }
        else
        {
            responseMessage = new ActionResponseMessage(MessageType.Error, "Ne postoji vrsta uloge koja ima ID " + id);
        }

        Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
        return responseMessage.MessageType == MessageType.Success ? new EmptyResult() : await Get(id);
    }

    /// <summary>
    /// Prikazuje formu za uređivanje postojeće vrste uloge.
    /// </summary>
    /// <param name="id">ID vrste uloge</param>
    /// <param name="page">Broj stranice</param>
    /// <param name="sort">Kriterij po kojem se sortira</param>
    /// <param name="ascending">Smjer sortiranja</param>
    /// <returns>Forma za uređivanje vrste uloge.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        var vrstaUloge = await ctx.VrstaUloge.FindAsync(id);
        if (vrstaUloge == null)
        {
            return NotFound("Ne postoji vrsta uloge s traženim ID-om: " + id);
        }

        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        return View(vrstaUloge);
    }

    /// <summary>
    /// Potvrđuje izmjenu vrste uloge i sprema promjene u bazu podataka.
    /// </summary>
    /// <param name="model">Izmijenjeni podaci za vrstu uloge</param>
    /// <param name="page">Broj stranice</param>
    /// <param name="sort">Kriterij po kojem se sortira</param>
    /// <param name="ascending">Smjer sortiranja</param>
    /// <returns>Poruku o ishodu i preusmjeravanje na Index.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VrstaUloge model, int page = 1, int sort = 1, bool ascending = true)
    {
        if (model == null)
        {
            return NotFound("Nema poslanih podataka");
        }

        var vrstaUloge = await ctx.VrstaUloge.FindAsync(model.Id);
        if (vrstaUloge == null)
        {
            return NotFound("Ne postoji vrsta uloge s navedenim ID-om " + vrstaUloge.Id);
        }

        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;

        if (ModelState.IsValid)
        {
            try
            {
                CopyValues(model, vrstaUloge);
                ctx.Update(vrstaUloge);
                await ctx.SaveChangesAsync();
                logger.LogInformation($"Vrsta uloge: {model.Id} uspješno ažurirana");
                TempData[Constants.Message] = $"Vrsta uloge {model.Id} uspješno ažurirana";
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
    /// Dohvaća djelomični prikaz s podacima o jednoj vrsti uloge.
    /// </summary>
    /// <param name="id">ID korisničkog računa</param>
    /// <returns>PartialView prikaz</returns>
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var vrstaUloge = await ctx.VrstaUloge.FindAsync(id);
        if (vrstaUloge != null)
        {
            return PartialView(VrstaUlogeViewModelMapper(vrstaUloge));
        }

        return NotFound($"Ne postoji vrsta uloge s traženim ID-om: " + id);
    }

    #region Private methods

    private static VrstaUlogeViewModel VrstaUlogeViewModelMapper(VrstaUloge vrstaUloge)
    {
        return new VrstaUlogeViewModel
        {
            Id = vrstaUloge.Id,
            Vrsta = vrstaUloge.Vrsta
        };
    }

    private void CopyValues(VrstaUloge model, VrstaUloge vrstaUloge)
    {
        vrstaUloge.Id = model.Id;
        vrstaUloge.Vrsta = model.Vrsta;
    }

    #endregion
}