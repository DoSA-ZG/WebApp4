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
    /// Kontroler za upravljanje vrstama projekata.
    /// </summary>
    public class VrstaProjektaController : Controller
    {

        private readonly Rppp04Context ctx;
        private readonly ILogger<VrstaDokumentaController> logger;
        private readonly AppSettings appData;

        /// <summary>
        /// Konstruktor kontrolera za upravljanje vrstama projekata.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka</param>
        /// <param name="options">Opcije aplikacije za straničenje.</param>
        /// <param name="logger">Logger za zapisivanje događaja</param>
        public VrstaProjektaController(Rppp04Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<VrstaDokumentaController> logger)
        {
            this.ctx = ctx;
            this.appData = options.Value;
            this.logger = logger;
        }

        /// <summary>
        /// Prikazuje popis vrsta projekata.
        /// </summary>
        /// <param name="page">Broj trenutne stranice</param>
        /// <param name="sort">Sortiranje</param>
        /// <param name="ascending">Poredak sortiranja</param>
        /// <returns>View sa popisom vrsta projekata</returns>
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appData.PageSize;

            var query = ctx.VrstaProjekta.AsNoTracking();
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

            var vrsteprojekta = await query
                          .Select(n => new VrstaProjektaViewModel
                          {
                              Id = n.Id,
                              Vrsta = n.Vrsta
                          })
                          .Skip((page - 1) * pagesize)
                          .Take(pagesize)
                          .ToListAsync();

            var model = new VrsteProjektaViewModel
            {
                vrsteProjekta = vrsteprojekta,
                PagingInfo = pagingInfo
            };


            return View(model);
        }

        /// <summary>
        /// Prikazuje formu za dodavanje nove vrste projekta.
        /// </summary>
        /// <returns>View sa formom za dodavanje</returns>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Dodaje novu vrstu projekta u bazu podataka.
        /// </summary>
        /// <param name="vrstaprojekta">Model vrste projekta</param>
        /// <returns>Redirekcija na akciju Index nakon dodavanja</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(VrstaProjekta vrstaprojekta)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(vrstaprojekta);
                    ctx.SaveChanges();

                    logger.LogInformation($"Vrsta projekta {vrstaprojekta.Vrsta} uspješno dodana.");
                    TempData[Constants.Message] = $"Vrsta projekta {vrstaprojekta.Vrsta} uspješno dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstaprojekta);
                }
            }
            else
            {
                return View(vrstaprojekta);
            }
        }

        /// <summary>
        /// Prikazuje formu za uređivanje postojeće vrste projekta.
        /// </summary>
        /// <param name="id">ID vrste projekta</param>
        /// <returns>PartialView sa formom za uređivanje</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vrstaprojekta = await ctx.VrstaProjekta
                                  .AsNoTracking()
                                  .Where(n => n.Id == id)
                                  .SingleOrDefaultAsync();
            if (vrstaprojekta != null)
            {
                return PartialView(vrstaprojekta);
            }
            else
            {
                return NotFound($"Neispravan ID vrste projekta: {id}" + ".");
            }
        }

        /// <summary>
        /// Ažurira postojeću vrstu projekta u bazi podataka.
        /// </summary>
        /// <param name="vrstaprojekta">Model vrste projekta</param>
        /// <returns>Redirekcija na akciju Get nakon ažuriranja</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(VrstaProjekta vrstaprojekta)
        {
            if (vrstaprojekta == null)
            {
                return NotFound("Nema poslanih podataka.");
            }
            bool checkId = await ctx.VrstaProjekta.AnyAsync(n => n.Id == vrstaprojekta.Id);
            if (!checkId)
            {
                return NotFound($"Neispravan ID vrste projekta: {vrstaprojekta?.Id}" + ".");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(vrstaprojekta);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Vrsta projekta {vrstaprojekta.Vrsta} uspješno ažurirana.");
                    return RedirectToAction(nameof(Get), new { id = vrstaprojekta.Id });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(vrstaprojekta);
                }
            }
            else
            {
                return PartialView(vrstaprojekta);
            }
        }

        /// <summary>
        /// Prikazuje informacije o vrsti projekta.
        /// </summary>
        /// <param name="id">ID vrste projekta</param>
        /// <returns>PartialView sa informacijama o vrsti projekta</returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var vrstaprojekta = await ctx.VrstaProjekta
                                  .Where(n => n.Id == id)
                                  .Select(n => new VrstaProjektaViewModel
                                  {
                                      Id = n.Id,
                                      Vrsta = n.Vrsta
                                  })
                                  .SingleOrDefaultAsync();
            if (vrstaprojekta != null)
            {
                return PartialView(vrstaprojekta);
            }
            else
            {
                return NotFound($"Neispravan ID vrste projekta: {id}.");
            }
        }

        /// <summary>
        /// Briše vrstu projekta iz baze podataka.
        /// </summary>
        /// <param name="id">ID vrste projekta</param>
        /// <returns>Rezultat akcije DELETE</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var vrstaprojekta = await ctx.VrstaProjekta.FindAsync(id);
            if (vrstaprojekta != null)
            {
                try
                {
                    string naziv = vrstaprojekta.Vrsta;
                    ctx.Remove(vrstaprojekta);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Vrsta projekta {vrstaprojekta.Vrsta} uspješno obrisana.");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Vrsta projekta {naziv} sa ID {id} uspješno obrisana.");
                }
                catch (Exception exc)
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja vrste projekta: {exc.CompleteExceptionMessage()}.");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Vrsta projekta sa ID {id} ne postoji.");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return responseMessage.MessageType == MessageType.Success ?
              new EmptyResult() : await Get(id);
        }


    }
}