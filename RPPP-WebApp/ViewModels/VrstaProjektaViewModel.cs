using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class VrstaProjektaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Vrsta dokumenta")]
        public string Vrsta { get; set; }
    }
}
