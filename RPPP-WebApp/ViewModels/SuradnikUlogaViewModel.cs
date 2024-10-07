using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels;

public class SuradnikUlogaViewModel
{
    public int Id { get; set; }

    public DateTime DatumPocetak { get; set; }

    public DateTime DatumKraj { get; set; }

    public Projekt Projekt { get; set; }

    public Suradnik Suradnik { get; set; }

    public VrstaUloge VrstaUloge { get; set; }
}