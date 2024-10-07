using RPPP_WebApp.Model;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class DokumentViewModel
    {

        public int Id { get; set; }

        [Display(Name = "Naziv dokumenta")]
        [Required(ErrorMessage = "Potrebno je unijeti naziv dokumenta")]
        public string NazivDok { get; set; }

        [Display(Name = "Vrsta dokumenta")]
        public int? VrstaDokumentaId { get; set; }
        public string VrstaDokumenta { get; set; }

        [Display(Name = "Status dokumenta")]
        public int? StatusDokumentaId { get; set; }
        public string StatusDokumenta { get; set; }

        [Display(Name = "Ekstenzija dokumenta")]
        public string EkstenzijaDokumenta { get; set; }

        [Display(Name = "Vrijeme prijenosa")]
        public DateTime? VrPrijenos {  get; set; }

        [Display(Name = "Datum zadnje izmjene")]
        public DateTime? DatumZadIzmj { get; set; }

        public int? ProjektId { get; set; }
        [Display(Name = "Naziv projekta")]
        public string NazivProjekt { get; set; }

        public IFormFile DatotekaFile { get; set; }

        public byte[] Datoteka { get; set; }

    }
}
