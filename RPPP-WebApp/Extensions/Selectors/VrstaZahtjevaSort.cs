using RPPP_WebApp.Model;
using System;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors
{

    /// <summary>
    /// Klasa za sortiranje vrste zahtjeva.
    /// </summary>
    public static class VrstaZahtjevaSort
    {

        /// <summary>
        /// Sortira vrste zahtjeva.
        /// </summary>
        /// <param name="query">Upit za vrste zahtjeva.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Sortirane vrste zahtjeva.</returns>
        public static IQueryable<VrstaZahtjeva> ApplySort(this IQueryable<VrstaZahtjeva> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<VrstaZahtjeva, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = a => a.NazivVrsteZahtjeva;
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


