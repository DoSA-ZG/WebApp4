
using RPPP_WebApp.Model;
namespace RPPP_WebApp.ViewModels {
public class VrsteTransakcijeViewModel
    {
        public IEnumerable<VrstaTransakcijeViewModel> VrsteTransakcije { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }

}
