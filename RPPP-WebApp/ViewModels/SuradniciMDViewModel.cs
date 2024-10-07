namespace RPPP_WebApp.ViewModels;

public class SuradniciMDViewModel
{
    public IEnumerable<SuradnikMasterViewModel> Suradnici { get; set; }
    public PagingInfo PagingInfo { get; set; }
}