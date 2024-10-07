using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class PrioritetiZadatakaViewModel
    {
        public IEnumerable<PrioritetZadatka> PrioritetiZadataka { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
