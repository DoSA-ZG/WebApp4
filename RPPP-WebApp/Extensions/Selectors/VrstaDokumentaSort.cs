using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors
{

    /// <summary>
    /// Klasa za sortiranje vrsta dokumenata.
    /// </summary>
    public static class VrstaDokumentaSort
    {

        /// <summary>
        /// Primjenjuje sortiranje na upit za vrste dokumenata.
        /// </summary>
        /// <param name="query">Upit za vrste dokumenata</param>
        /// <param name="sort">Indeks stupca po kojem se vrši sortiranje</param>
        /// <param name="ascending">Poredak sortiranja (u uzlaznom redoslijedu ili ne)</param>
        /// <returns>Sortirani upit za vrste dokumenata</returns>
        public static IQueryable<VrstaDokumenta> ApplySort(this IQueryable<VrstaDokumenta> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<VrstaDokumenta, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = p => p.Id;
                    break;
                case 2:
                    orderSelector = p => p.VrstaDok;
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