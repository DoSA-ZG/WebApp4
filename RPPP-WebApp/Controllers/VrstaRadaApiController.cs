using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.Util.ExceptionFilters;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers;

/// <summary>
/// Upravljač za REST-api za vrste rada
/// </summary>
[ApiController]
[Route("[controller]")]
[TypeFilter(typeof(ProblemDetailsForSqlException))]
public class VrstaRadaApiController : ControllerBase, ICustomController<int, VrstaRadaViewModel>
{
    private readonly Rppp04Context ctx;

    private static Dictionary<string, Expression<Func<VrstaRada, object>>> orderSelectors = new()
    {
        [nameof(VrstaRadaViewModel.Id).ToLower()] = m => m.Id,
        [nameof(VrstaRadaViewModel.VrstaRada).ToLower()] = m => m.VrstaRada1
    };

    private static Expression<Func<VrstaRada, VrstaRadaViewModel>> projection = m => new VrstaRadaViewModel
    {
        Id = m.Id,
        VrstaRada = m.VrstaRada1
    };

    public VrstaRadaApiController(Rppp04Context ctx)
    {
        this.ctx = ctx;
    }


    /// <summary>
    /// Broj svih vrsta radova
    /// </summary>
    /// <param name="filter">Opcionalni filter za vrste rada</param>
    /// <returns></returns>
    [HttpGet("count", Name = "BrojVrstaRada")]
    public async Task<int> Count([FromQuery] string filter)
    {
        var query = ctx.VrstaRada.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(m => m.VrstaRada1.Contains(filter));
        }

        int count = await query.CountAsync();
        return count;
    }

    /// <summary>
    /// Dohvat vrsti rada uz opcionalni filter
    /// </summary>
    /// <param name="loadParams">Postavke za straničenje i filter</param>
    /// <returns></returns>
    [HttpGet(Name = "DohvatiVrsteRada")]
    public async Task<List<VrstaRadaViewModel>> GetAll([FromQuery] LoadParams loadParams)
    {
        var query = ctx.VrstaRada.AsQueryable();

        if (!string.IsNullOrWhiteSpace(loadParams.Filter))
        {
            query = query.Where(m => m.VrstaRada1.Contains(loadParams.Filter));
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
    /// Vraća vrstu rada određenu sa id-om
    /// </summary>
    /// <param name="id">Id vrste rada koja se dohvaća</param>
    /// <returns></returns>
    [HttpGet("{id}", Name = "DohvatiVrstuRada")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VrstaRadaViewModel>> Get(int id)
    {
        var vrstaRada = await ctx.VrstaRada
            .Where(m => m.Id == id)
            .Select(projection)
            .FirstOrDefaultAsync();
        if (vrstaRada == null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
        }
        else
        {
            return vrstaRada;
        }
    }

    /// <summary>
    /// Brisanje vrste rada određene s id
    /// </summary>
    /// <param name="id">Vrijednost id od vrste rada</param>
    /// <returns></returns>
    /// <response code="204">Ako je vrsta rada uspješno obrisana</response>
    /// <response code="404">Ako vrsta rada s poslanim id-om ne postoji</response> 
    [HttpDelete("{id}", Name = "ObrisiVrstuRada")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        Console.WriteLine(id);
        var vrstaRada = await ctx.VrstaRada.FindAsync(id);
        if (vrstaRada == null)
        {
            return NotFound();
        }
        else
        {
            ctx.Remove(vrstaRada);
            await ctx.SaveChangesAsync();
            return NoContent();
        }

        ;
    }


    /// <summary>
    /// Ažurira vrstu rada
    /// </summary>
    /// <param name="id">parametar čija vrijednost jednoznačno identificira vrstu rada</param>
    /// <param name="model">Podaci o vrsti rada. model.Id mora se podudarati s parametrom id</param>
    /// <returns></returns>
    [HttpPut("{id}", Name = "AzurirajVrstuRada")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, VrstaRadaViewModel model)
    {
        if (model.Id != id) //ModelState.IsValid i model != null provjera se automatski zbog [ApiController]
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                detail: $"Different ids {id} vs {model.Id}");
        }
        else
        {
            var vrstaRada = await ctx.VrstaRada.FindAsync(id);
            if (vrstaRada == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
            }

            CopyValues(vrstaRada, model);

            await ctx.SaveChangesAsync();
            return NoContent();
        }
    }

    /// <summary>
    /// Stvara novu vrstu rada opisom poslanim modelom
    /// </summary>
    /// <param name="model">Podaci o vrsti rada</param>
    /// <returns></returns>
    [HttpPost(Name = "DodajVrstuRada")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(VrstaRadaViewModel model)
    {
        VrstaRada vrstaRada = new VrstaRada
        {
            Id = model.Id,
            VrstaRada1 = model.VrstaRada
        };
        ctx.Add(vrstaRada);
        await ctx.SaveChangesAsync();

        var addedItem = await Get(vrstaRada.Id);

        return CreatedAtAction(nameof(Get), new { id = vrstaRada.Id }, addedItem.Value);
    }

    private void CopyValues(VrstaRada vrstaRada, VrstaRadaViewModel model)
    {
        vrstaRada.Id = model.Id;
        vrstaRada.VrstaRada1 = model.VrstaRada;
    }
}