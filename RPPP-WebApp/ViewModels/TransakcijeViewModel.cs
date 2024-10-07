using RPPP_WebApp.Model;
namespace RPPP_WebApp.ViewModels {
    public class TransakcijeViewModel
    {
        public IEnumerable<TransakcijaViewModel> Transakcije { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }

}
