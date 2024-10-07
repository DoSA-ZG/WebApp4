using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.Util.ExceptionFilters;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers;

/// <summary>
/// Upravljač za REST-api za vrste uloga
/// </summary>
[ApiController]
[Route("[controller]")]
[TypeFilter(typeof(ProblemDetailsForSqlException))]
public class VrstaUlogeApiController : ControllerBase, ICustomController<int, VrstaUlogeViewModel>
{
    private readonly Rppp04Context ctx;

    private static Dictionary<string, Expression<Func<VrstaUloge, object>>> orderSelectors = new()
    {
        [nameof(VrstaUlogeViewModel.Id).ToLower()] = m => m.Id,
        [nameof(VrstaUlogeViewModel.Vrsta).ToLower()] = m => m.Vrsta
    };

    private static Expression<Func<VrstaUloge, VrstaUlogeViewModel>> projection = m => new VrstaUlogeViewModel
    {
        Id = m.Id,
        Vrsta = m.Vrsta
    };

    public VrstaUlogeApiController(Rppp04Context ctx)
    {
        this.ctx = ctx;
    }


    /// <summary>
    /// Vraća broj svih vrsta uloga filtriran prema nazivu vrste 
    /// </summary>
    /// <param name="filter">Opcionalni filter za vrste uloga</param>
    /// <returns></returns>
    [HttpGet("count", Name = "BrojVrstaUloga")]
    public async Task<int> Count([FromQuery] string filter)
    {
        var query = ctx.VrstaUloge.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(m => m.Vrsta.Contains(filter));
        }

        int count = await query.CountAsync();
        return count;
    }

    /// <summary>
    /// Dohvat vrsta uloga (opcionalno filtrirano po nazivu vrste).
    /// Broj vrsta, poredak, početna pozicija određeni s loadParams.
    /// </summary>
    /// <param name="loadParams">Postavke za straničenje i filter</param>
    /// <returns></returns>
    [HttpGet(Name = "DohvatiVrsteUloga")]
    public async Task<List<VrstaUlogeViewModel>> GetAll([FromQuery] LoadParams loadParams)
    {
        var query = ctx.VrstaUloge.AsQueryable();

        if (!string.IsNullOrWhiteSpace(loadParams.Filter))
        {
            query = query.Where(m => m.Vrsta.Contains(loadParams.Filter));
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
    /// Vraća vrstu uloge čiji je Id jednak vrijednosti parametra id
    /// </summary>
    /// <param name="id">IdVrsteUloge</param>
    /// <returns></returns>
    [HttpGet("{id}", Name = "DohvatiVrstuUloge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VrstaUlogeViewModel>> Get(int id)
    {
        var vrstaUloge = await ctx.VrstaUloge
            .Where(m => m.Id == id)
            .Select(projection)
            .FirstOrDefaultAsync();
        if (vrstaUloge == null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
        }
        else
        {
            return vrstaUloge;
        }
    }


    /// <summary>
    /// Brisanje vrste uloge određene s id
    /// </summary>
    /// <param name="id">Vrijednost primarnog ključa (Id vrste uloge)</param>
    /// <returns></returns>
    /// <response code="204">Ako je vrsta uloge uspješno obrisana</response>
    /// <response code="404">Ako vrsta uloge s poslanim id-om ne postoji</response>      
    [HttpDelete("{id}", Name = "ObrisiVrstuUloge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        Console.WriteLine(id);
        var vrstaUloge = await ctx.VrstaUloge.FindAsync(id);
        if (vrstaUloge == null)
        {
            return NotFound();
        }
        else
        {
            ctx.Remove(vrstaUloge);
            await ctx.SaveChangesAsync();
            return NoContent();
        }

        ;
    }

    /// <summary>
    /// Ažurira vrstu uloge
    /// </summary>
    /// <param name="id">parametar čija vrijednost jednoznačno identificira mjesto</param>
    /// <param name="model">Podaci o vrsti uloge. IdVrsteUloge mora se podudarati s parametrom id</param>
    /// <returns></returns>
    [HttpPut("{id}", Name = "AzurirajVrstuUloge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, VrstaUlogeViewModel model)
    {
        if (model.Id != id) //ModelState.IsValid i model != null provjera se automatski zbog [ApiController]
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                detail: $"Different ids {id} vs {model.Id}");
        }
        else
        {
            var vrstaUloge = await ctx.VrstaUloge.FindAsync(id);
            if (vrstaUloge == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
            }

            CopyValues(vrstaUloge, model);

            await ctx.SaveChangesAsync();
            return NoContent();
        }
    }

    /// <summary>
    /// Stvara novu vrstu uloge opisom poslanim modelom
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost(Name = "DodajVrstuUloge")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(VrstaUlogeViewModel model)
    {
        VrstaUloge vrstaUloge = new VrstaUloge
        {
            Id = model.Id,
            Vrsta = model.Vrsta
        };
        ctx.Add(vrstaUloge);
        await ctx.SaveChangesAsync();

        var addedItem = await Get(vrstaUloge.Id);

        return CreatedAtAction(nameof(Get), new { id = vrstaUloge.Id }, addedItem.Value);
    }

    private void CopyValues(VrstaUloge vrstaUloge, VrstaUlogeViewModel model)
    {
        vrstaUloge.Id = model.Id;
        vrstaUloge.Vrsta = model.Vrsta;
    }
}