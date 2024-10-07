using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class TransakcijaViewModel
    {
        public int Id { get; set; }
        public int? Iznos { get; set; }
        public string Iban { get; set; }
        public DateTime? DatumVrijeme { get; set; }
        public int KarticaFrom { get; set; }
        public int KarticaTo { get; set; }

        public VrstaTransakcije Vrsta { get; set; }
    }
}