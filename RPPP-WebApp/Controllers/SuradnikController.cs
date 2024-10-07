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
/// Kontroler za upravljanje suradnicima
/// </summary>
public class SuradnikController : Controller
{
    private readonly Rppp04Context ctx;
    private readonly ILogger<SuradnikController> logger;
    private readonly AppSettings appData;

    public SuradnikController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options,
        ILogger<SuradnikController> logger)
    {
        this.ctx = ctx;
        this.logger = logger;
        appData = options.Value;
    }

    /// <summary>
    /// Prikazuje listu suradnika s opcijama za straničenje i sortiranje.
    /// </summary>
    /// <param name="page">Broj stranice.</param>
    /// <param name="sort">Vrsta sortiranja.</param>
    /// <param name="ascending">Smjer sortiranja.</param>
    /// <returns>Prikazuje listu suradnika.</returns>
    public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appData.PageSize;

        var query = ctx.Suradnik.AsQueryable();
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

        var suradnici = await query
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .Include(suradnik => suradnik.VrstaSuradnika)
            .Include(suradnik => suradnik.KorisnickiRacun)
            .Select(suradnik => SuradnikViewModelMapper(suradnik))
            .ToListAsync();
        var model = new SuradniciViewModel()
        {
            Suradnici = suradnici,
            PagingInfo = pagingInfo
        };

        return View(model);
    }


    /// <summary>
    /// Prikazuje formu za dodavanje novog suradnika.
    /// </summary>
    /// <returns>Prikazuje formu za unos novog suradnika.</returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PrepareDropDownLists();
        return View();
    }

    /// <summary>
    /// Dodaje novog suradnika u bazu podataka.
    /// </summary>
    /// <param name="model">Podaci o suradniku.</param>
    /// <returns>Preusmjerava na akciju Index nakon dodavanja.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Suradnik model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                ctx.Add(model);
                await ctx.SaveChangesAsync();
                logger.LogInformation(new EventId(2011), $"Suradnik {model.Ime} {model.Prezime} dodan.");
                TempData[Constants.Message] = $"Suradnik uspješno dodan. Id={model.Id}";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom dodavanje novog suradnika: {0}",
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
    /// Briše suradnika s određenim ID-jem
    /// </summary>
    /// <param name="id">ID suradnika</param>
    /// <returns>Poruku za obavještavanje korisnika o ishodu brisanja.</returns>
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        ActionResponseMessage responseMessage;
        var suradnik = await ctx.Suradnik.FindAsync(id);
        if (suradnik != null)
        {
            try
            {
                string imePrezimeSuradnika = $"{suradnik.Ime} {suradnik.Prezime}";
                ctx.Remove(suradnik);
                await ctx.SaveChangesAsync();
                responseMessage =
                    new ActionResponseMessage(MessageType.Success, $"Suradnik {imePrezimeSuradnika} uspješno obrisan.");
            }
            // catch (DbUpdateException e) when ((e.InnerException as SqlException)?.Number == 547)
            // {
            //     responseMessage = new ActionResponseMessage(MessageType.Error,
            //         "Nije moguće ukloniti suradnika dok se ne uklone sve njegove uloge na svim projektima.");
            // }
            catch (Exception exc)
            {
                responseMessage = new ActionResponseMessage(MessageType.Error,
                    "Pogreška prilikom brisanja suradnika: " + exc.CompleteExceptionMessage());
            }
        }
        else
        {
            responseMessage = new ActionResponseMessage(MessageType.Error, "Ne postoji suradnik koji ima ID " + id);
        }

        Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
        return responseMessage.MessageType == MessageType.Success ? new EmptyResult() : await Get(id);
    }

    /// <summary>
    /// Prikazuje formu za uređivanje postojećeg suradnika.
    /// </summary>
    /// <param name="id">ID suradnika</param>
    /// <param name="page">Broj stranice</param>
    /// <param name="sort">Kriterij po kojem se sortira</param>
    /// <param name="ascending">Smjer sortiranja</param>
    /// <returns>Forma za uređivanje suradnika.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        var suradnik = await ctx.Suradnik.FindAsync(id);
        if (suradnik == null)
        {
            return NotFound("Ne postoji suradnik s traženim ID-om: " + id);
        }

        await PrepareDropDownLists();
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        return View(suradnik);
    }

    /// <summary>
    /// Potvrđuje izmjenu suradnika i sprema promjene u bazu podataka.
    /// </summary>
    /// <param name="model">Izmijenjeni podaci za suradnika</param>
    /// <param name="page">Broj stranice</param>
    /// <param name="sort">Kriterij po kojem se sortira</param>
    /// <param name="ascending">Smjer sortiranja</param>
    /// <returns>Poruku o ishodu i preusmjeravanje na Index.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Suradnik model, int page = 1, int sort = 1, bool ascending = true)
    {
        if (model == null)
        {
            return NotFound("Nema poslanih podataka");
        }

        var suradnik = await ctx.Suradnik.FindAsync(model.Id);
        if (suradnik == null)
        {
            return NotFound("Ne postoji suradnik s navedenim ID-om " + suradnik.Id);
        }

        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;

        if (ModelState.IsValid)
        {
            try
            {
                CopyValues(model, suradnik);
                ctx.Update(suradnik);
                await ctx.SaveChangesAsync();
                logger.LogInformation($"Suradnik: {model.Id} uspješno ažuriran");
                TempData[Constants.Message] = $"Suradnik {model.Id} uspješno ažuriran";
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
    /// Dohvaća djelomični prikaz s podacima o jednom suradniku.
    /// </summary>
    /// <param name="id">ID suradnika</param>
    /// <returns>PartialView prikaz</returns>
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var suradnik = await ctx.Suradnik.FindAsync(id);
        if (suradnik != null)
        {
            return PartialView(SuradnikViewModelMapper(suradnik));
        }

        return NotFound($"Ne postoji suradnik s traženim ID-om: " + id);
    }

    #region Private methods

    private async Task PrepareDropDownLists()
    {
        var vrsteSuradnika = await ctx.VrstaSuradnika
            .Select(vs => new { vs.Id, vs.Vrsta })
            .ToListAsync();
        ViewBag.VrsteSuradnika =
            new SelectList(vrsteSuradnika, nameof(VrstaSuradnika.Id), nameof(VrstaSuradnika.Vrsta));
        var korisnickiRacuni = await ctx.KorisnickiRacun
            .Select(vs => new { vs.Id, vs.StupanjPrava })
            .ToListAsync();
        var selectListItems = korisnickiRacuni.Select(racun => new SelectListItem
        {
            Value = racun.Id.ToString(),
            Text = $"ID: {racun.Id}  (Stupanj prava: {racun.StupanjPrava})"
        }).ToList();

        ViewBag.KorisnickiRacuni = new SelectList(selectListItems, "Value", "Text");
    }

    private static SuradnikViewModel SuradnikViewModelMapper(Suradnik suradnik)
    {
        return new SuradnikViewModel
        {
            Id = suradnik.Id,
            Organizacija = suradnik.Organizacija,
            Email = suradnik.Email,
            Ime = suradnik.Ime,
            Prezime = suradnik.Prezime,
            BrojTelefona = suradnik.BrojTelefona,
            KorisnickiRacun = suradnik.KorisnickiRacun,
            VrstaSuradnika = suradnik.VrstaSuradnika
        };
    }

    private void CopyValues(Suradnik model, Suradnik suradnik)
    {
        suradnik.Id = model.Id;
        suradnik.Ime = model.Ime;
        suradnik.Prezime = model.Prezime;
        suradnik.BrojTelefona = model.BrojTelefona;
        suradnik.Email = model.Email;
        suradnik.Organizacija = model.Organizacija;
        suradnik.VrstaSuradnikaId = model.VrstaSuradnikaId;
        suradnik.KorisnickiRacunId = model.KorisnickiRacunId;
    }

    #endregion
}