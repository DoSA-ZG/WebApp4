using RPPP_WebApp.Model;
using System;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors
{

    /// <summary>
    /// Klasa za sortiranje prioriteta zahtjeva.
    /// </summary>
    public static class PrioritetZahtjevaSort
    {

        /// <summary>
        /// Sortira prioritete zahtjeva.
        /// </summary>
        /// <param name="query">Upit za prioritete zahtjeva.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Sortirani prioriteti zahtjeva.</returns>
        public static IQueryable<PrioritetZahtjeva> ApplySort(this IQueryable<PrioritetZahtjeva> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<PrioritetZahtjeva, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = a => a.NazivPrioritetaZahtjeva;
                    break;
                case 2:
                    orderSelector = a => a.StupanjPrioriteta;
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


