using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels;

public class SuradnikMDViewModel
{
    public int Id { get; set; }

    public string Organizacija { get; set; }

    public string Email { get; set; }

    public string Ime { get; set; }

    public string Prezime { get; set; }

    public string BrojTelefona { get; set; }

    public int? KorisnickiRacunId { get; set; }
    public virtual KorisnickiRacun KorisnickiRacun { get; set; }

    public int VrstaSuradnikaId { get; set; }
    public virtual VrstaSuradnika VrstaSuradnika { get; set; }
    public IEnumerable<SuradnikUloga> Stavke { get; set; } 
}