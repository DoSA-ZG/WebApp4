using System.ComponentModel.DataAnnotations;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class ZadatakVModel
    {
        public int Id { get; set; }

        public string OpisZadatak { get; set; }
        [DataType(DataType.Date)]
        public DateTime PlanPocetak { get; set; }
        [DataType(DataType.Date)]
        public DateTime PlanKraj { get; set; }
        [DataType(DataType.Date)]
        public DateTime? StvarniPocetak { get; set; }
        [DataType(DataType.Date)]
        public DateTime? StvarniKraj { get; set; }

        public string Email { get; set; }

        public string NazivPrioriteta { get; set; }

        public string Status { get; set; }
        public int? IdPrethDokumenta { get; set; }

        public int PrioritetZadatkaId { get; set; }

        public int StatusZadatkaId { get; set; }

        public int ProjektniZahtjevId { get; set; }

        public int SuradnikId { get; set; }

        public IEnumerable<EvidencijaRadaViewModel> Evidencije { get; set; }

        public ZadatakVModel()
        {
            this.Evidencije = new List<EvidencijaRadaViewModel>();
        }
    }
}

