using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class EvidencijaSort
    {
        public static IQueryable<EvidencijaRada> ApplySort(this IQueryable<EvidencijaRada> query, int sort, bool ascending)
        {
            Expression<Func<EvidencijaRada, object>> orderSelector = sort switch
            {
                1 => d => d.BrojSati,
                2 => d => d.OpisRada,
                3 => d => d.Zadatak.OpisZadatak,
                4 => d => d.VrstaRada.VrstaRada1,
                5 => d => d.Suradnik.Email,
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
