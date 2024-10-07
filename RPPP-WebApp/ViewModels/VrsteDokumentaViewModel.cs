namespace RPPP_WebApp.ViewModels
{
    public class VrsteDokumentaViewModel
    {
        public IEnumerable<VrstaDokumentaViewModel> vrsteDokumenta { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
