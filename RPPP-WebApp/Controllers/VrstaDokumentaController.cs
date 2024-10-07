using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Text.Json;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje vrstama dokumenata.
    /// </summary>
    public class VrstaDokumentaController : Controller
    {

        private readonly Rppp04Context ctx;
        private readonly ILogger<VrstaDokumentaController> logger;
        private readonly AppSettings appData;

        /// <summary>
        /// Konstruktor kontrolera za upravljanje vrstama dokumenata.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka</param>
        /// <param name="options">Opcije aplikacije vezano za straničenje</param>
        /// <param name="logger">Logger za zapisivanje događaja</param>
        public VrstaDokumentaController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<VrstaDokumentaController> logger)
        {
            this.ctx = ctx;
            this.appData = options.Value;
            this.logger = logger;
        }

        /// <summary>
        /// Prikazuje popis vrsta dokumenata.
        /// </summary>
        /// <param name="page">Broj trenutne stranice</param>
        /// <param name="sort">Indeks stupca po kojem se vrši sortiranje</param>
        /// <param name="ascending">Poredak sortiranja</param>
        /// <returns>View sa popisom vrsta dokumenata</returns>
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.VrstaDokumenta.AsNoTracking();
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
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var vrstedokumenta = await query
                          .Select(n => new VrstaDokumentaViewModel
                          {
                              Id = n.Id,
                              VrstaDok = n.VrstaDok
                          })
                          .Skip((page - 1) * pagesize)
                          .Take(pagesize)
                          .ToListAsync();

            var model = new VrsteDokumentaViewModel
            {
                vrsteDokumenta = vrstedokumenta,
                PagingInfo = pagingInfo
            };


            return View(model);
        }

        /// <summary>
        /// Prikazuje formu za dodavanje nove vrste dokumenta.
        /// </summary>
        /// <returns>View sa formom za dodavanje</returns>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Dodaje novu vrstu dokumenta u bazu podataka.
        /// </summary>
        /// <param name="vrstadokumenta">Model vrste dokumenta</param>
        /// <returns>Redirekcija na akciju Index nakon dodavanja</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(VrstaDokumenta vrstadokumenta)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(vrstadokumenta);
                    ctx.SaveChanges();

                    logger.LogInformation($"Vrsta dokumenta {vrstadokumenta.VrstaDok} uspješno dodana.");
                    TempData[Constants.Message] = $"Vrsta dokumenta {vrstadokumenta.VrstaDok} uspješno dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstadokumenta);
                }
            }
            else
            {
                return View(vrstadokumenta);
            }
        }

        /// <summary>
        /// Prikazuje formu za uređivanje postojeće vrste dokumenta.
        /// </summary>
        /// <param name="id">ID vrste dokumenta</param>
        /// <returns>PartialView sa formom za uređivanje</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vrstadokumenta = await ctx.VrstaDokumenta
                                  .AsNoTracking()
                                  .Where(n => n.Id == id)
                                  .SingleOrDefaultAsync();
            if (vrstadokumenta != null)
            {
                return PartialView(vrstadokumenta);
            }
            else
            {
                return NotFound($"Neispravan ID vrste dokumenta: {id}" + ".");
            }
        }

        /// <summary>
        /// Ažurira postojeću vrstu dokumenta u bazi podataka.
        /// </summary>
        /// <param name="vrstadokumenta">Model vrste dokumenta</param>
        /// <returns>Redirekcija na akciju Get nakon ažuriranja</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(VrstaDokumenta vrstadokumenta)
        {
            if (vrstadokumenta == null)
            {
                return NotFound("Nema poslanih podataka.");
            }
            bool checkId = await ctx.VrstaDokumenta.AnyAsync(n => n.Id == vrstadokumenta.Id);
            if (!checkId)
            {
                return NotFound($"Neispravan ID vrste dokumenta: {vrstadokumenta?.Id}" + ".");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(vrstadokumenta);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Vrsta dokumenta {vrstadokumenta.VrstaDok} uspješno ažurirana.");
                    return RedirectToAction(nameof(Get), new { id = vrstadokumenta.Id });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(vrstadokumenta);
                }
            }
            else
            {
                return PartialView(vrstadokumenta);
            }
        }

        /// <summary>
        /// Prikazuje informacije o odabranoj vrsti dokumenta.
        /// </summary>
        /// <param name="id">ID vrste dokumenta</param>
        /// <returns>PartialView sa informacijama o vrsti dokumenta</returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var vrstadokumenta = await ctx.VrstaDokumenta
                                  .Where(n => n.Id == id)
                                  .Select(n => new VrstaDokumentaViewModel
                                  {
                                      Id = n.Id,
                                      VrstaDok = n.VrstaDok
                                  })
                                  .SingleOrDefaultAsync();
            if (vrstadokumenta != null)
            {
                return PartialView(vrstadokumenta);
            }
            else
            {
                return NotFound($"Neispravan ID vrste dokumenta: {id}.");
            }
        }

        /// <summary>
        /// Briše vrstu dokumenta iz baze podataka.
        /// </summary>
        /// <param name="id">ID vrste dokumenta</param>
        /// <returns>Rezultat akcije DELETE</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var vrstadokumenta = await ctx.VrstaDokumenta.FindAsync(id);
            if (vrstadokumenta != null)
            {
                try
                {
                    string naziv = vrstadokumenta.VrstaDok;
                    ctx.Remove(vrstadokumenta);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Vrsta dokumenta {vrstadokumenta.VrstaDok} uspješno obrisana.");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Vrsta dokumenta {naziv} sa ID {id} uspješno obrisana.");
                }
                catch (Exception exc)
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja vrste dokumenta: {exc.CompleteExceptionMessage()}.");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Vrsta dokumenta sa ID {id} ne postoji.");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return responseMessage.MessageType == MessageType.Success ?
              new EmptyResult() : await Get(id);
        }
    }
}