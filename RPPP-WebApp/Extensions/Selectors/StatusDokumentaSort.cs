using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors
{

    /// <summary>
    /// Klasa za sortiranje statusa dokumenata.
    /// </summary>
    public static class StatusDokumentaSort
    {
        /// <summary>
        /// Primjenjuje sortiranje na upit za status dokumenata.
        /// </summary>
        /// <param name="query">Upit za status dokumenata</param>
        /// <param name="sort">Indeks stupca po kojem se vrši sortiranje</param>
        /// <param name="ascending">Poredak sortiranja (u uzlaznom redoslijedu ili ne)</param>
        /// <returns>Sortirani upit za status dokumenata</returns>
        public static IQueryable<StatusDokumenta> ApplySort(this IQueryable<StatusDokumenta> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<StatusDokumenta, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = p => p.Id;
                    break;
                case 2:
                    orderSelector = p => p.StatusDok;
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