using RPPP_WebApp.Model;
namespace RPPP_WebApp.ViewModels
{
    public class ProjektniZahtjeviViewModel
    {
        public IEnumerable<ProjektniZahtjevViewModel> ProjektniZahtjevi { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
