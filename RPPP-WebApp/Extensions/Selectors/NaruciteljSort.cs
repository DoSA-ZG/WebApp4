using RPPP_WebApp.Model;
using System;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors
{
    /// <summary>
    /// Klasa za sortiranje upita za naručitelje.
    /// </summary>
    public static class NaruciteljSort
    {

        /// <summary>
        /// Primjenjuje sortiranje na upit za naručitelje.
        /// </summary>
        /// <param name="query">Upit za naručitelje</param>
        /// <param name="sort">Indeks stupca po kojem se vrši sortiranje</param>
        /// <param name="ascending">Poredak sortiranja (u uzlaznom redoslijedu ili ne)</param>
        /// <returns>Sortirani upit za naručitelje</returns>
        public static IQueryable<Narucitelj> ApplySort(this IQueryable<Narucitelj> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Narucitelj, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = p => p.Id;
                    break;
                case 2:
                    orderSelector = p => p.NazivNarucitelj;
                    break;
                case 3:
                    orderSelector = p => p.Oib;
                    break;
                case 4:
                    orderSelector = p => p.Iban;
                    break;
                case 5:
                    orderSelector = p => p.Adresa;
                    break;
                case 6:
                    orderSelector = p => p.Email;
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