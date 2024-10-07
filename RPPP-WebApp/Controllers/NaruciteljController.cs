using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.Util.ExceptionFilters;
using RPPP_WebApp.ViewModels;
using System.Linq.Expressions;
using System.Text.Json;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Web API servis za rad s naručiteljima
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [TypeFilter(typeof(ProblemDetailsForSqlException))]
    public class NaruciteljController : ControllerBase, ICustomController<int, NaruciteljViewModel>
    {
        private readonly Rppp04Context ctx;
        private static Dictionary<string, Expression<Func<Narucitelj, object>>> orderSelectors = new()
        {
            [nameof(NaruciteljViewModel.Id).ToLower()] = n => n.Id,
            [nameof(NaruciteljViewModel.NazivNarucitelj).ToLower()] = n => n.NazivNarucitelj,
            [nameof(NaruciteljViewModel.Iban).ToLower()] = n => n.Iban,
            [nameof(NaruciteljViewModel.Oib).ToLower()] = n => n.Oib,
            [nameof(NaruciteljViewModel.Adresa).ToLower()] = n => n.Adresa,
            [nameof(NaruciteljViewModel.Email).ToLower()] = n => n.Email
        };

        private static Expression<Func<Narucitelj, NaruciteljViewModel>> projection = n => new NaruciteljViewModel
        {
            Id = n.Id,  
            NazivNarucitelj = n.NazivNarucitelj,
            Iban = n.Iban,
            Oib = n.Oib,
            Adresa = n.Adresa,
            Email = n.Email
        };

        public NaruciteljController(Rppp04Context ctx)
        {
            this.ctx = ctx;
        }

        /// <summary>
        /// Vraća broj svih naručitelja filtriran prema nazivu naručitelja 
        /// </summary>
        /// <param name="filter">Opcionalni filter za naziv naručitelja</param>
        /// <returns></returns>
        [HttpGet("count", Name = "BrojNarucitelja")]
        public async Task<int> Count([FromQuery] string filter)
        {
            var query = ctx.Narucitelj.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(m => m.NazivNarucitelj.Contains(filter));
            }
            int count = await query.CountAsync();
            return count;
        }

        /// <summary>
        /// Dohvat naručitelja (opcionalno filtrirano po nazivu naručitelja).
        /// Broj naručitelja, poredak, početna pozicija određeni s loadParams.
        /// </summary>
        /// <param name="loadParams">Postavke za straničenje i filter</param>
        /// <returns></returns>
        [HttpGet(Name = "DohvatiNarucitelje")]
        public async Task<List<NaruciteljViewModel>> GetAll([FromQuery] LoadParams loadParams)
        {
            var query = ctx.Narucitelj.AsQueryable();

            if (!string.IsNullOrWhiteSpace(loadParams.Filter))
            {
                query = query.Where(n => n.NazivNarucitelj.Contains(loadParams.Filter));
            }

            if (loadParams.SortColumn != null)
            {
                if (orderSelectors.TryGetValue(loadParams.SortColumn.ToLower(), out var expr))
                {
                    query = loadParams.Descending ? query.OrderByDescending(expr) : query.OrderBy(expr);
                }
            }

            var list = await query.Select(projection)
                                  .Skip(loadParams.StartIndex)
                                  .Take(loadParams.Rows)
                                  .ToListAsync();
            return list;
        }

        /// <summary>
        /// Vraća grad čiji je IDNarucitelj jednak vrijednosti parametra id
        /// </summary>
        /// <param name="id">ID naručitelja</param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "DohvatiNarucitelja")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NaruciteljViewModel>> Get(int id)
        {
            var narucitelj = await ctx.Narucitelj
                                  .Where(n => n.Id == id)
                                  .Select(projection)
                                  .FirstOrDefaultAsync();
            if (narucitelj == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
            }
            else
            {
                return narucitelj;
            }
        }

        /// <summary>
        /// Brisanje naručitelja određenog s ID
        /// </summary>
        /// <param name="id">Vrijednost primarnog ključa (ID naručitelja)</param>
        /// <returns></returns>
        /// <response code="204">Ako je naručitelj uspješno obrisan</response>
        /// <response code="404">Ako naručitelj s poslanim ID-om ne postoji</response>   
        [HttpDelete("{id}", Name = "ObrisiNarucitelja")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var narucitelj = await ctx.Narucitelj.FindAsync(id);
            if (narucitelj == null)
            {
                return NotFound();
            }
            else
            {
                ctx.Remove(narucitelj);
                await ctx.SaveChangesAsync();
                return NoContent();
            };
        }


        /// <summary>
        /// Ažurira naručitelja
        /// </summary>
        /// <param name="id">parametar čija vrijednost jednoznačno identificira naručitelja</param>
        /// <param name="model">Podaci o naručitelju. IDNarucitelj mora se podudarati s parametrom id</param>
        /// <returns></returns>
        [HttpPut("{id}", Name = "AzurirajNarucitelja")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, NaruciteljViewModel model)
        {
            if (model.Id != id)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Različiti IDs: {id} vs {model.Id}");
            }
            else
            {
                var narucitelj = await ctx.Narucitelj.FindAsync(id);
                if (narucitelj == null)
                {
                    return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Pogrešan ID = {id}");
                }

                narucitelj.NazivNarucitelj = model.NazivNarucitelj;
                narucitelj.Iban = model.Iban;
                narucitelj.Adresa = model.Adresa;
                narucitelj.Email = model.Email;

                await ctx.SaveChangesAsync();
                return NoContent();
            }
        }

        /// <summary>
        /// Stvara novog naručitelja opisom poslanim modelom
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost(Name = "DodajNarucitelja")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(NaruciteljViewModel model)
        {
            Narucitelj narucitelj = new Narucitelj
            {
                NazivNarucitelj = model.NazivNarucitelj,
                Iban = model.Iban,
                Adresa = model.Adresa,
                Oib = model.Oib,
                Email = model.Email
            };
            ctx.Add(narucitelj);
            await ctx.SaveChangesAsync();

            var addedItem = await Get(narucitelj.Id);

            return CreatedAtAction(nameof(Get), new { id = narucitelj.Id }, addedItem.Value);
        }
    }
}