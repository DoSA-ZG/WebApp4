using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors
{
    /// <summary>
    /// Klasa za sortiranje vrsta projekata.
    /// </summary>
    public static class VrstaProjektaSort
    {

        /// <summary>
        /// Primjenjuje sortiranje na upit za vrste projekata.
        /// </summary>
        /// <param name="query">Upit za vrste projekata</param>
        /// <param name="sort">Indeks stupca po kojem se vrši sortiranje</param>
        /// <param name="ascending">Poredak sortiranja (u uzlaznom redoslijedu ili ne)</param>
        /// <returns>Sortirani upit za vrste projekata</returns>
        public static IQueryable<VrstaProjekta> ApplySort(this IQueryable<VrstaProjekta> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<VrstaProjekta, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = p => p.Id;
                    break;
                case 2:
                    orderSelector = p => p.Vrsta;
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