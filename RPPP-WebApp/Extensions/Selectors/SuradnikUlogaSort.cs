using RPPP_WebApp.Model;

namespace RPPP_WebApp.Extensions.Selectors;

/// <summary>
/// Klasa za sortiranje suradnik uloga po različitim atributima prilikom dohvaćanja iz upita.
/// </summary>
public static class SuradnikUlogaSort
{
    public static IQueryable<SuradnikUloga> ApplySort(this IQueryable<SuradnikUloga> query, int sort, bool ascending)
    {
        System.Linq.Expressions.Expression<Func<SuradnikUloga, object>> orderSelector = sort switch
        {
            1 => p => p.DatumPocetak,
            2 => p => p.DatumKraj,
            3 => p => p.Projekt.NazivProjekt,
            4 => p => $"{p.Suradnik.Ime} {p.Suradnik.Prezime}",
            5 => p => p.VrstaUloge.Vrsta,
            _ => null
        };
        if (orderSelector != null)
        {
            query = ascending ?
                query.OrderBy(orderSelector) :
                query.OrderByDescending(orderSelector);
        }

        return query;
    }
}