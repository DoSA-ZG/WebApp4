using RPPP_WebApp.Model;

namespace RPPP_WebApp.Extensions.Selectors;

/// <summary>
/// Klasa za sortiranje vrsti uloga po različitim atributima prilikom dohvaćanja iz upita.
/// </summary>
public static class VrstaUlogeSort
{
    public static IQueryable<VrstaUloge> ApplySort(this IQueryable<VrstaUloge> query, int sort, bool ascending)
    {
        System.Linq.Expressions.Expression<Func<VrstaUloge, object>> orderSelector = sort switch
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