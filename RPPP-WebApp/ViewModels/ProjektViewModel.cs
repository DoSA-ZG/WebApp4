using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektViewModel
    {

        public int IdProjekt { get; set; }

        [Display(Name = "Naziv projekta")]
        [Required(ErrorMessage = "Potrebno je unijeti naziv projekta")]
        public string NazivProjekt { get; set; }

        [Display(Name = "Kratica projekta")]
        public string KraticaProjekt { get; set; }

        [Display(Name = "Datum isporuke projekta")]
        [Required(ErrorMessage = "Potrebno je unijeti datum isporuke projekta")]
        public DateTime? DatumIsporukaPr { get; set; }

        [Display(Name = "Naziv naručitelja")]
        public int? NaruciteljId { get; set; }
        [Required(ErrorMessage = "Potrebno je unijeti naručitelja projekta")]
        public string NazivNarucitelj { get; set; }

        [Display(Name = "Vrsta projekta")]
        public int? VrstaProjektaId {  get; set; }
        [Required(ErrorMessage = "Potrebno je unijeti vrstu projekta")]
        public string Vrsta { get; set; }

        public IEnumerable<DokumentViewModel> Dokumenti { get; set; }

        public IEnumerable<DokumentViewModel> NoviDokumenti { get; set; }

        public ProjektViewModel()
        {
            this.Dokumenti = new List<DokumentViewModel>();
        }


    }
}
