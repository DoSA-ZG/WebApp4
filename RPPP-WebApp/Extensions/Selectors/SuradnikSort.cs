using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Extensions.Selectors;

/// <summary>
/// Klasa za sortiranje suradnika po različitim atributima prilikom dohvaćanja iz upita.
/// </summary>
public static class SuradnikSort
{
  public static IQueryable<Suradnik> ApplySort(this IQueryable<Suradnik> query, int sort, bool ascending)
  {
    System.Linq.Expressions.Expression<Func<Suradnik, object>> orderSelector = sort switch
    {
      1 => p => p.Ime,
      2 => p => p.Prezime,
      3 => p => p.BrojTelefona,
      4 => p => p.Email,
      5 => p => p.Organizacija,
      6 => p => p.VrstaSuradnika.Vrsta,
      7 => p => p.KorisnickiRacun.StupanjPrava,
      _ => null
    };
    if (orderSelector != null)
    {
      query = ascending ?
        query.OrderBy(orderSelector) :
        query.OrderByDescending(orderSelector);
    }

    return query;
  }
}