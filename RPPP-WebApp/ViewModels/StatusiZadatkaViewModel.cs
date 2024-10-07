using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class StatusiZadatkaViewModel
    {
        
            public IEnumerable<StatusZadatka> statusi { get; set; }
            public PagingInfo PagingInfo { get; set; }
     
    }
}

