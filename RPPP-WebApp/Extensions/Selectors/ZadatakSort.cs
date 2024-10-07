using RPPP_WebApp.Model;
using RPPP_WebApp.ModelsPartial;
using RPPP_WebApp.ViewModels;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class ZadatakSort
    {
        public static IQueryable<ViewZadatak> ApplySort(this IQueryable<ViewZadatak> query, int sort, bool ascending)
        {
            Expression<Func<ViewZadatak, object>> orderSelector = sort switch
            {
                1 => d => d.OpisZadatak,
                2 => d => d.PlanPocetak,
                3 => d => d.PlanKraj,
                4 => d => d.StvarniPocetak,
                5 => d => d.StvarniKraj,
                6 => d => d.Email,
                7 => d => d.StupanjPrioriteta,
                8 => d => d.Status,
                9 => d => d.Id,
                10 => d => d.UkSati,
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
