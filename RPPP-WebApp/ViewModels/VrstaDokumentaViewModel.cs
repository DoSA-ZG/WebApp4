using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class VrstaDokumentaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Vrsta dokumenta")]
        public string VrstaDok { get; set; }
    }
}
