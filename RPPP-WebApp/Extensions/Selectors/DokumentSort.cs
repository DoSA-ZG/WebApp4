using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors
{
    /// <summary>
    /// Klasa za sortiranje upita za dokumente.
    /// </summary>
    public static class DokumentSort
    {

        /// <summary>
        /// Primjenjuje sortiranje na upit za dokumente.
        /// </summary>
        /// <param name="query">Upit za dokumente</param>
        /// <param name="sort">Indeks stupca po kojem se vrši sortiranje</param>
        /// <param name="ascending">Poredak sortiranja (u uzlaznom redoslijedu ili ne)</param>
        /// <returns>Sortirani upit za dokumente</returns>
        public static IQueryable<Dokument> ApplySort(this IQueryable<Dokument> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Dokument, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = p => p.Id;
                    break;
                case 2:
                    orderSelector = p => p.NazivDok;
                    break;
                case 3:
                    orderSelector = p => p.IdVrstaDokNavigation.VrstaDok;
                    break;
                case 4:
                    orderSelector = p => p.IdStatusDokNavigation.StatusDok;
                    break;
                case 5:
                    orderSelector = p => p.EkstenzijaDokumenta;
                    break;
                case 6:
                    orderSelector = p => p.VrPrijenos;
                    break;
                case 7:
                    orderSelector = p => p.DatumZadIzmj;
                    break;
                case 8:
                    orderSelector = p => p.IdProjektNavigation.NazivProjekt;
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