using RPPP_WebApp.Model;

namespace RPPP_WebApp.Extensions.Selectors;

/// <summary>
/// Klasa za sortiranje vrsti suradnika po različitim atributima prilikom dohvaćanja iz upita.
/// </summary>
public static class VrstaSuradnikaSort
{
    public static IQueryable<VrstaSuradnika> ApplySort(this IQueryable<VrstaSuradnika> query, int sort, bool ascending)
    {
        System.Linq.Expressions.Expression<Func<VrstaSuradnika, object>> orderSelector = sort switch
        {
            1 => p => p.Vrsta,
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