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
    /// Kontroler za upravljanje statusima dokumenata.
    /// </summary>
    public class StatusDokumentaController : Controller
    {

        private readonly Rppp04Context ctx;
        private readonly ILogger<StatusDokumentaController> logger;
        private readonly AppSettings appData;

        /// <summary>
        /// Konstruktor kontrolera za upravljanje statusima dokumenata.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka</param>
        /// <param name="options">Opcije aplikacije vezano za straničenje</param>
        /// <param name="logger">Logger za zapisivanje događaja</param>
        public StatusDokumentaController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<StatusDokumentaController> logger)
        {
            this.ctx = ctx;
            this.appData = options.Value;
            this.logger = logger;
        }

        /// <summary>
        /// Prikazuje popis statusa dokumenata.
        /// </summary>
        /// <param name="page">Broj trenutne stranice</param>
        /// <param name="sort">Indeks stupca po kojem se vrši sortiranje</param>
        /// <param name="ascending">Poredak sortiranja</param>
        /// <returns>View sa popisom statusa dokumenata</returns>
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.StatusDokumenta.AsNoTracking();
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

            var statusidokumenta = await query
                          .Select(n => new StatusDokumentaViewModel
                          {
                              Id = n.Id,
                              StatusDok = n.StatusDok
                          })
                          .Skip((page - 1) * pagesize)
                          .Take(pagesize)
                          .ToListAsync();

            var model = new StatusiDokumentaViewModel
            {
                statusiDokumenta = statusidokumenta,
                PagingInfo = pagingInfo
            };


            return View(model);
        }

        /// <summary>
        /// Prikazuje formu za dodavanje novog statusa dokumenta.
        /// </summary>
        /// <returns>View sa formom za dodavanje</returns>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Dodaje novi status dokumenta u bazu podataka.
        /// </summary>
        /// <param name="statusdokumenta">Model statusa dokumenta</param>
        /// <returns>Redirekcija na akciju Index nakon dodavanja</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StatusDokumenta statusdokumenta)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(statusdokumenta);
                    ctx.SaveChanges();

                    logger.LogInformation($"Status dokumenta {statusdokumenta.StatusDok} uspješno dodan.");
                    TempData[Constants.Message] = $"Status dokumenta {statusdokumenta.StatusDok} uspješno dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(statusdokumenta);
                }
            }
            else
            {
                return View(statusdokumenta);
            }
        }

        /// <summary>
        /// Prikazuje formu za uređivanje postojećeg statusa dokumenta.
        /// </summary>
        /// <param name="id">ID statusa dokumenta</param>
        /// <returns>PartialView sa formom za uređivanje</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var statusdokumenta = await ctx.StatusDokumenta
                                  .AsNoTracking()
                                  .Where(n => n.Id == id)
                                  .SingleOrDefaultAsync();
            if (statusdokumenta != null)
            {
                return PartialView(statusdokumenta);
            }
            else
            {
                return NotFound($"Neispravan ID statusa dokumenta: {id}" + ".");
            }
        }

        /// <summary>
        /// Ažurira postojeći status dokumenta u bazi podataka.
        /// </summary>
        /// <param name="statusdokumenta">Model statusa dokumenta</param>
        /// <returns>Redirekcija na akciju Get nakon ažuriranja</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(StatusDokumenta statusdokumenta)
        {
            if (statusdokumenta == null)
            {
                return NotFound("Nema poslanih podataka.");
            }
            bool checkId = await ctx.StatusDokumenta.AnyAsync(m => m.Id == statusdokumenta.Id);
            if (!checkId)
            {
                return NotFound($"Neispravan ID statusa dokumenta: {statusdokumenta?.Id}" + ".");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(statusdokumenta);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Status dokumenta {statusdokumenta.StatusDok} sa ID {statusdokumenta.Id} uspješno ažuriran.");
                    return RedirectToAction(nameof(Get), new { id = statusdokumenta.Id });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(statusdokumenta);
                }
            }
            else
            {
                return PartialView(statusdokumenta);
            }
        }

        /// <summary>
        /// Prikazuje informacije o odabranom statusu dokumenta.
        /// </summary>
        /// <param name="id">ID statusa dokumenta</param>
        /// <returns>PartialView sa informacijama o statusu dokumenta</returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var statusdokumenta = await ctx.StatusDokumenta
                                  .Where(n => n.Id == id)
                                  .Select(n => new StatusDokumentaViewModel
                                  {
                                      Id = n.Id,
                                      StatusDok = n.StatusDok
                                  })
                                  .SingleOrDefaultAsync();
            if (statusdokumenta != null)
            {
                return PartialView(statusdokumenta);
            }
            else
            {
                return NotFound($"Neispravan ID statusa dokumenta: {id}.");
            }
        }

        /// <summary>
        /// Briše status dokumenta iz baze podataka.
        /// </summary>
        /// <param name="id">ID statusa dokumenta</param>
        /// <returns>Rezultat akcije</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var statusdokumenta = await ctx.StatusDokumenta.FindAsync(id);
            if (statusdokumenta != null)
            {
                try
                {
                    string naziv = statusdokumenta.StatusDok;
                    ctx.Remove(statusdokumenta);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Status dokumenta {naziv} sa ID {id} uspješno obrisan.");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Status dokumenta {naziv} sa ID {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja statusa dokumenta: {exc.CompleteExceptionMessage()}.");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Status dokumenta sa ID {id} ne postoji.");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return responseMessage.MessageType == MessageType.Success ?
              new EmptyResult() : await Get(id);
        }


    }
}