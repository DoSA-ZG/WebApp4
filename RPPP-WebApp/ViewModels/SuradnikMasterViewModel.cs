namespace RPPP_WebApp.ViewModels;

public class SuradnikMasterViewModel
{
    public SuradnikViewModel Suradnik { get; set; }
    public IEnumerable<SuradnikUlogaViewModel> Stavke { get; set; } 
}