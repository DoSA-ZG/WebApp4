
using RPPP_WebApp.Model;
using RPPP_WebApp.ModelsPartial;

namespace RPPP_WebApp.ViewModels {
public class ZadatciViewModel
    {
        public IEnumerable<ViewZadatak> Zadatci { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }

}
