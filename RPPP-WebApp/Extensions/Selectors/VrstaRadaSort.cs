


using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class VrstaRadaSort
    {
        public static IQueryable<VrstaRada> ApplySort(this IQueryable<VrstaRada> query, int sort, bool ascending)
        {
            Expression<Func<VrstaRada, object>> orderSelector = sort switch
            {
                1 => d => d.Id,
                2 => d => d.VrstaRada1,
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
}