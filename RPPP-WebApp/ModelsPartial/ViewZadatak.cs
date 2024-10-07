using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.ModelsPartial
{
    public class ViewZadatak
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

        public int StupanjPrioriteta { get; set; }
        public int? UkSati { get; set; }

        [NotMapped]
        public int Position { get; set; }
    }
}
