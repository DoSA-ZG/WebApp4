namespace RPPP_WebApp.ViewModels;

public class VrsteUlogaViewModel
{
    public IEnumerable<VrstaUlogeViewModel> VrsteUloga { get; set; }
    public PagingInfo PagingInfo { get; set; }
}