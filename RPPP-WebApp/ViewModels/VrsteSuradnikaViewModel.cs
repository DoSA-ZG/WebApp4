namespace RPPP_WebApp.ViewModels;

public class VrsteSuradnikaViewModel
{
    public IEnumerable<VrstaSuradnikaViewModel> VrsteSuradnika { get; set; }
    public PagingInfo PagingInfo { get; set; }
}