namespace RPPP_WebApp.ViewModels;

public class SuradniciViewModel
{
    public IEnumerable<SuradnikViewModel> Suradnici { get; set; }
    public PagingInfo PagingInfo { get; set; }
}