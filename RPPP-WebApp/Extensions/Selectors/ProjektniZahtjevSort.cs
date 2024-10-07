using RPPP_WebApp.Model;
using System;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors
{
    /// <summary>
    /// Klasa za sortiranje projektnih zahtjeva.
    /// </summary>
    public static class ProjektniZahtjevSort
    {

        /// <summary>
        /// Sortira projektne zahtjeve.
        /// </summary>
        /// <param name="query">Upit za projektne zahtjeve.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Sortirani projektni zahtjevi.</returns>
        public static IQueryable<ProjektniZahtjev> ApplySort(this IQueryable<ProjektniZahtjev> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<ProjektniZahtjev, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = a => a.NazivZahtjeva;
                    break;
                case 2:
                    orderSelector = a => a.OpisZahtjeva;
                    break;
                case 3:
                    orderSelector = a => a.PrioritetZahtjeva.NazivPrioritetaZahtjeva;
                    break;
                case 4:
                    orderSelector = a => a.VrstaZahtjeva.NazivVrsteZahtjeva;
                    break;
                case 5:
                    orderSelector = a => a.Projekt.NazivProjekt;
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

