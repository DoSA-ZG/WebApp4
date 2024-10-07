using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class VrstaTransakcijeSort
    {
        public static IQueryable<VrstaTransakcije> ApplySort(this IQueryable<VrstaTransakcije> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<VrstaTransakcije, object>> orderSelector = null;
                switch (sort)
                {
                    case 1:
                    orderSelector = t => t.Vrsta;
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
