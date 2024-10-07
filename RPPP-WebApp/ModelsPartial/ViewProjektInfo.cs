using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model
{
    public class ViewProjektInfo
    {

        public int IdProjekt { get; set; }

        public string NazivProjekt { get; set; }

        public int? VrstaProjektId { get; set; }
        public string Vrsta { get; set; }

        public DateTime? DatumIsporukaPr {  get; set; }

        public int? NaruciteljId { get; set; }
        public string NazivNarucitelj { get; set; }

        [NotMapped]
        public int Position {  get; set; }

        [NotMapped]
        public List<Dokument> Dokumenti { get; set; }

    }
}
