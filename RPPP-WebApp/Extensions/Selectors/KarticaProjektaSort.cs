using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class KarticaProjektaSort
    {
        public static IQueryable<KarticaProjekta> ApplySort(this IQueryable<KarticaProjekta> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<KarticaProjekta, object>> orderSelector = null;
                switch (sort)
                {
                    case 1:
                    orderSelector = t => t.ProjektId;
                    break;
                }
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
