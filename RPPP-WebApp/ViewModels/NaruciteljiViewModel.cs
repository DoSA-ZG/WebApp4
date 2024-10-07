using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class NaruciteljiViewModel
    {
        public IEnumerable<NaruciteljViewModel> Narucitelji { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }
}
