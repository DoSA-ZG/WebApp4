using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class TransakcijaSort
    {
        public static IQueryable<Transakcija> ApplySort(this IQueryable<Transakcija> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Transakcija, object>> orderSelector = null;
                switch (sort)
                {
                    case 1:
                    orderSelector = t => t.Iznos;
                    break;
                    case 2:
                    orderSelector = t => t.Iban;
                    break;
                    case 3:
                    orderSelector = t => t.DatumVrijeme;
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
