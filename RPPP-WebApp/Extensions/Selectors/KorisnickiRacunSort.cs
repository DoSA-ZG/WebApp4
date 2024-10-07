using RPPP_WebApp.Model;

namespace RPPP_WebApp.Extensions.Selectors;

/// <summary>
/// Klasa za sortiranje korisničkih računa po različitim atributima prilikom dohvaćanja iz upita.
/// </summary>
public static class KorisnickiRacunSort
{
    public static IQueryable<KorisnickiRacun> ApplySort(this IQueryable<KorisnickiRacun> query, int sort, bool ascending)
    {
        System.Linq.Expressions.Expression<Func<KorisnickiRacun, object>> orderSelector = sort switch
        {
            1 => p => p.Id,
            2 => p => p.StupanjPrava,
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