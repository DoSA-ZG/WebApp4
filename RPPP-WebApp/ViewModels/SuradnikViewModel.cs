using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels;

public class SuradnikViewModel
{
    public int Id { get; set; }

    public string Organizacija { get; set; }

    public string Email { get; set; }

    public string Ime { get; set; }

    public string Prezime { get; set; }

    public string BrojTelefona { get; set; }

    public KorisnickiRacun? KorisnickiRacun { get; set; }

    public VrstaSuradnika VrstaSuradnika { get; set; }
    
}