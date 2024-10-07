using RPPP_WebApp.Model;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektiViewModel
    {
        public IEnumerable<ViewProjektInfo> Projekti { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }
}
