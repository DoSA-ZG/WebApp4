using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektiReportModel
    {

        public int IdProjekt { get; set; }
        public string NazivProjekt { get; set; }

        public DateTime? DatumIsporukaPr { get; set; }

        public string NazivNarucitelj { get; set; }

        public string Vrsta { get; set; }

        public String NaziviDokumenata { get; set; }


    }
}
