using RPPP_WebApp.Model;
using System;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors
{
    /// <summary>
    /// Klasa za sortiranje upita za projekte.
    /// </summary>
    public static class ProjektSort
    {

        /// <summary>
        /// Primjenjuje sortiranje na upit za projekte.
        /// </summary>
        /// <param name="query">Upit za projekte</param>
        /// <param name="sort">Indeks stupca po kojem se vrši sortiranje</param>
        /// <param name="ascending">Poredak sortiranja (u uzlaznom redoslijedu ili ne)</param>
        /// <returns>Sortirani upit za projekte</returns>
        public static IQueryable<ViewProjektInfo> ApplySort(this IQueryable<ViewProjektInfo> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<ViewProjektInfo, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.IdProjekt;
                    break;
                case 2:
                    orderSelector = d => d.NazivProjekt;
                    break;
                case 3:
                    orderSelector = d => d.Vrsta;
                    break;
                case 4:
                    orderSelector = d => d.DatumIsporukaPr;
                    break;
                case 5:
                    orderSelector = d => d.NazivNarucitelj;
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