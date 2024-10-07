using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class VrsteRadaViewModel
    {

        public IEnumerable<VrstaRada> radovi { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }
}
