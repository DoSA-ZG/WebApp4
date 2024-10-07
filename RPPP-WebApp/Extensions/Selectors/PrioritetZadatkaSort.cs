using RPPP_WebApp.Model;
using System;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors
{

    /// <summary>
    /// Klasa za sortiranje prioriteta zadataka.
    /// </summary>
    public static class PrioritetZadatkaSort
    {

        /// <summary>
        /// Sortira prioriteta zadataka.
        /// </summary>
        /// <param name="query">Upit za prioritete zadataka.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Sortirani prioriteti zadataka.</returns>
        public static IQueryable<PrioritetZadatka> ApplySort(this IQueryable<PrioritetZadatka> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<PrioritetZadatka, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = a => a.NazivPrioriteta;
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


