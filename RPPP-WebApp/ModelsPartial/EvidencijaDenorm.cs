using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.ModelsPartial
{
    public class EvidencijaDenorm
    {
        public int ZadatakId { get; set; }

        public string EmailNositelj { get; set; }

        public string Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime PlanPocetak { get; set; }
       
       
        [DataType(DataType.Date)]
        public DateTime? PlanKraj { get; set; }

        public int BrojSati { get; set; }
        public string OpisRada { get; set; }
        public string OpisZadatak { get; set; }
        public string VrstaRada { get; set; }
        public string EmailRad { get; set; }

        [NotMapped]
        public string UrlZadatka { get; set;}

    }
}
