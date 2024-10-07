using NLog.Web.LayoutRenderers;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektniZahtjevViewModel
    {
        public int Id { get; set; }

        public string NazivZahtjeva { get; set; }

        public string OpisZahtjeva { get; set; }

        public string PrioritetZahtjeva { get; set; }

        public PrioritetZahtjeva PrioritetZahtjevaObj { get; set; }

        public int PrioritetZahtjevaId { get; set; }

        public string VrstaZahtjeva { get; set; }

        public VrstaZahtjeva VrstaZahtjevaObj { get; set; }

        public int VrstaZahtjevaId { get; set; }

        public string Projekt { get; set; }

        public Projekt ProjektObj { get; set; }

        public int ProjektId { get; set; }

        public string NaziviZadataka { get; set; }

        public IEnumerable<ZadatakViewModel> Zadatci { get; set; }

        public ProjektniZahtjevViewModel()
        {
            this.Zadatci = new List<ZadatakViewModel>();
        }
    }
}
