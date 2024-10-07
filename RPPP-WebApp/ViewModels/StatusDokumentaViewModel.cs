using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class StatusDokumentaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Status dokumenta")]
        public string StatusDok { get; set; }
    }
}
