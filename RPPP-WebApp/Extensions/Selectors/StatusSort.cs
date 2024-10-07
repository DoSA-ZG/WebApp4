using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Linq.Expressions;




namespace RPPP_WebApp.Extensions.Selectors
{
    public static class StatusSort
        {
        public static IQueryable<StatusZadatka> ApplySort(this IQueryable<StatusZadatka> query, int sort, bool ascending)
        {
            Expression<Func<StatusZadatka, object>> orderSelector = sort switch
            {
                1 => d => d.Id,
                2 => d => d.Status,
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
